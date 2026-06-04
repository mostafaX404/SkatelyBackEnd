using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

public class PaymentService(IUnitOfWork uow,
 ICartService cartService, IConfiguration config) : IPaymentService
{
    public async Task<ShoppingCart> CreateOrUpdatePaymentIntent(string cartId)
    {
        StripeConfiguration.ApiKey = config["SripeSettings:SecretKey"];
        var cart = await cartService.GetCartAsync(cartId);

        if (cart == null) return null;

        var ShippingPrice = (decimal)0;

        if (cart.DeliveryMethodId.HasValue)
        {
            var method = await uow.Repository<DeliveryMethod>().GetByIdAsync((int)cart.DeliveryMethodId);

            if (method is null) return null;

            ShippingPrice = method.Price;
        }

        foreach (var item in cart.Items)
        {
            var product = await uow.Repository<Core.Entities.Product>().GetByIdAsync(item.ProductId);
            if (product is null) return null;
            if (item.Price != product.Price)
            {
                item.Price = product.Price;
            }
        }

        var service = new PaymentIntentService();
        PaymentIntent? intent = null;

        if (string.IsNullOrEmpty(cart.PaymentIntentId))
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)cart.Items.Sum(x => x.Quantity * (x.Price * 100))
                    + (long)ShippingPrice * 100,
                Currency = "usd",
                PaymentMethodTypes = ["card"]
            };
            intent = await service.CreateAsync(options);
            cart.PaymentIntentId = intent.Id;
            cart.ClientSecret = intent.ClientSecret;
        }
        else
        {
            var existingIntent = await service.GetAsync(cart.PaymentIntentId);

            if (existingIntent.Status == "succeeded" ||
                existingIntent.Status == "canceled")
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)cart.Items.Sum(x => x.Quantity * (x.Price * 100))
                        + (long)ShippingPrice * 100,
                    Currency = "usd",
                    PaymentMethodTypes = ["card"]
                };
                intent = await service.CreateAsync(options);
                cart.PaymentIntentId = intent.Id;
                cart.ClientSecret = intent.ClientSecret;
            }
            else
            {
                var options = new PaymentIntentUpdateOptions
                {
                    Amount = (long)cart.Items.Sum(x => x.Quantity * (x.Price * 100))
                        + (long)ShippingPrice * 100
                };
                intent = await service.UpdateAsync(cart.PaymentIntentId, options);
            }
        }

        await cartService.SetCartAsync(cart);

        return cart;
    }
}