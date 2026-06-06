using API.Controllers;
using API.DTOs;
using API.Helpers;
using Core.Interfaces;
using Infrastructure.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles ="Admin")]
public class AdminController(IUnitOfWork unit , IPaymentService paymentService) : BaseApiController
{
    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrders([FromQuery] OrderSpecsParams specParams)
    {
        var spec = new OrderSpecification(specParams);

        return await CreatePagedResult(unit.Repository<Order>(), spec, specParams.PageIndex,
            specParams.PageSize , o=>o.ToDto());
    }

    [HttpGet("order/{id:int}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var spec = new OrderSpecification(id);

        var result = await unit.Repository<Order>().GetEntityWithSpec(spec);

        if(result == null) return BadRequest("no order with that id ");


        return result.ToDto();
        
    }


    [HttpPost("orders/refund/{id:int}")]
    public async Task<ActionResult<OrderDto>> RefundOrder(int id)
    {

        var spec = new OrderSpecification(id);
        var order = await unit.Repository<Order>().GetEntityWithSpec(spec);

        if(order is null) return BadRequest("no order with that id");

        var result = await paymentService.RefundPayment(order.PaymentIntentId);

        if(order.Status == OrderStatus.Pending) return BadRequest("Payment not recevied");

        if(result == "succeeded")
        {
            order.Status = OrderStatus.Refunded;

            await unit.Complete();

            return order.ToDto();
        }

        return BadRequest("problem during refunding");
    }

}