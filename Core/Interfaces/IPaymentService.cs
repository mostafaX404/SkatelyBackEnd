using Core.Entities;

public interface IPaymentService
{
    Task<ShoppingCart> CreateOrUpdatePaymentIntent(string cartId);

    Task<string> RefundPayment(string PatmentIntentId);
}