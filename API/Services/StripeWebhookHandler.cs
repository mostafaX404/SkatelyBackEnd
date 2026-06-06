using API.Extensions;
using API.Helpers;
using API.SignalR;
using Core.Interfaces;
using Infrastructure.Specifications;
using Microsoft.AspNetCore.SignalR;
using Stripe;

namespace API.Services;

public class StripeWebhookHandler(
    IUnitOfWork unit,
    IHubContext<NotificationHub> hubContext,
    ILogger<StripeWebhookHandler> logger)
{
    public async Task HandleEventAsync(Event stripeEvent)
    {
        logger.LogInformation("Processing Stripe webhook event {EventType} ({EventId})",
            stripeEvent.Type, stripeEvent.Id);

        switch (stripeEvent.Type)
        {
            case "payment_intent.succeeded":
                if (stripeEvent.Data.Object is PaymentIntent succeededIntent)
                    await UpdateOrderFromPaymentIntent(succeededIntent);
                break;

            case "payment_intent.payment_failed":
                if (stripeEvent.Data.Object is PaymentIntent failedIntent)
                    await UpdateOrderStatus(failedIntent.Id, OrderStatus.PaymentFailed);
                break;

            case "payment_intent.canceled":
                if (stripeEvent.Data.Object is PaymentIntent canceledIntent)
                    await UpdateOrderStatus(canceledIntent.Id, OrderStatus.PaymentFailed);
                break;

            case "charge.refunded":
                if (stripeEvent.Data.Object is Charge refundedCharge &&
                    !string.IsNullOrEmpty(refundedCharge.PaymentIntentId))
                    await UpdateOrderStatus(refundedCharge.PaymentIntentId, OrderStatus.Refunded);
                break;

            default:
                logger.LogInformation("Unhandled Stripe event type {EventType}", stripeEvent.Type);
                break;
        }
    }

    private async Task UpdateOrderFromPaymentIntent(PaymentIntent intent)
    {
        if (intent.Status != "succeeded") return;

        var order = await GetOrderByPaymentIntent(intent.Id);
        if (order is null)
        {
            logger.LogWarning("No order found for PaymentIntent {PaymentIntentId}", intent.Id);
            throw new InvalidOperationException($"Order not found for payment intent {intent.Id}");
        }

        var orderTotalInCents = PaymentAmountHelper.CalculateTotalInCents(
            order.Subtotal, order.DeliveryMethod.Price);

        var newStatus = orderTotalInCents != intent.Amount
            ? OrderStatus.PaymentMismatch
            : OrderStatus.PaymentReceived;

        if (orderTotalInCents != intent.Amount)
        {
            logger.LogWarning(
                "Payment amount mismatch for order {OrderId}. Expected {Expected} cents, got {Actual} cents",
                order.Id, orderTotalInCents, intent.Amount);
        }

        await PersistOrderStatus(order, newStatus);
    }

    private async Task UpdateOrderStatus(string paymentIntentId, OrderStatus status)
    {
        var order = await GetOrderByPaymentIntent(paymentIntentId);
        if (order is null)
        {
            logger.LogWarning("No order found for PaymentIntent {PaymentIntentId}", paymentIntentId);
            return;
        }

        await PersistOrderStatus(order, status);
    }

    private async Task<Order?> GetOrderByPaymentIntent(string paymentIntentId)
    {
        var spec = new OrderSpecification(paymentIntentId, true);
        return await unit.Repository<Order>().GetEntityWithSpec(spec);
    }

    private async Task PersistOrderStatus(Order order, OrderStatus status)
    {
        if (order.Status == status) return;

        order.Status = status;
        unit.Repository<Order>().Update(order);

        if (!await unit.Complete())
        {
            logger.LogError("Failed to save order {OrderId} status update to {Status}", order.Id, status);
            throw new InvalidOperationException($"Failed to update order {order.Id}");
        }

        logger.LogInformation("Order {OrderId} status updated to {Status}", order.Id, status);

        var connectionId = NotificationHub.GetConnectionIdByEmail(order.BuyerEmail);
        if (!string.IsNullOrEmpty(connectionId))
        {
            await hubContext.Clients.Client(connectionId)
                .SendAsync("OrderCompleteNotification", order.ToDto());
        }
    }
}
