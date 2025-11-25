using EcommerceCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCore.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Asp.Versioning.ApiVersion("1.0")]
public class PaymentsController(IPaymentService paymentService, ICartService cartService) : ControllerBase
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
}
