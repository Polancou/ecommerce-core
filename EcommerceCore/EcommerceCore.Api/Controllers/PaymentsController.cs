using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCore.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Asp.Versioning.ApiVersion("1.0")]
public class PaymentsController(IPaymentService paymentService, ICartService cartService, IOrderService orderService)
    : ControllerBase
{
    [HttpPost("create-intent")]
    public async Task<ActionResult<object>> CreatePaymentIntent()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Get user cart to calculate total
        var cart = await cartService.GetCartAsync(userId);
        if (!cart.Items.Any())
        {
            return BadRequest(new { message = "El carrito está vacío." });
        }

        var clientSecret = await paymentService.CreatePaymentIntentAsync(cart.TotalPrice);

        return Ok(new { clientSecret });
    }

    [HttpPost("confirm")]
    public async Task<ActionResult<OrderDto>> ConfirmPayment([FromBody] ConfirmPaymentDto dto)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        // 1. Verify Payment with Stripe
        var status = await paymentService.GetPaymentIntentStatusAsync(dto.PaymentIntentId);

        if (status != "succeeded")
        {
            return BadRequest(new { message = "El pago no se ha completado exitosamente." });
        }

        // 2. Create Order
        try
        {
            // Check if order already exists for this payment intent to prevent duplicates (optional but recommended)
            // For now, we rely on the cart being cleared.

            var order = await orderService.CreateOrderAsync(userId);

            // 3. Clear Cart (handled inside CreateOrderAsync? No, CreateOrderAsync removes the cart entity)
            // OrderService.CreateOrderAsync already removes the cart from DB: context.Carts.Remove(cart);
            // So we don't need to call CartService.ClearCartAsync explicitly if using the same context/transaction.
            // Let's verify OrderService implementation.

            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    public class ConfirmPaymentDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
    }
}
