using API.Services;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace API.Controllers;

public class PaymentsController(IPaymentService paymentService,
    IUnitOfWork unit,
    StripeWebhookHandler webhookHandler,
    ILogger<PaymentsController> logger,
    IConfiguration config) : BaseApiController
{
    private readonly string _whSecret = config["StripeSettings:WhSecret"]!;

    [Authorize]
    [HttpPost("{cartId}")]
    public async Task<ActionResult> CreateOrUpdatePaymentIntent(string cartId)
    {
        var cart = await paymentService.CreateOrUpdatePaymentIntent(cartId);

        if (cart == null) return BadRequest("Problem with your cart on the API");

        return Ok(cart);
    }

    [HttpGet("delivery-methods")]
    public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
    {
        return Ok(await unit.Repository<DeliveryMethod>().ListAllAsync());
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _whSecret,
                throwOnApiVersionMismatch: false);

            await webhookHandler.HandleEventAsync(stripeEvent);

            return Ok();
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe webhook signature verification failed");
            return BadRequest("Invalid Stripe signature");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Stripe webhook processing failed");
            return StatusCode(StatusCodes.Status500InternalServerError, "Webhook error");
        }
    }
}