using System.Security.Claims;
using Asp.Versioning;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCore.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    /// <summary>
    /// Obtiene los pedidos del usuario actual (o todos si es Admin).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var isAdmin = User.IsInRole("Admin");

        if (isAdmin)
        {
            return Ok(await orderService.GetAllOrdersAsync());
        }

        return Ok(await orderService.GetUserOrdersAsync(userId));
    }

    /// <summary>
    /// Obtiene un pedido específico por su ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var isAdmin = User.IsInRole("Admin");

        try
        {
            var order = await orderService.GetOrderByIdAsync(id, userId, isAdmin);
            if (order == null) return NotFound();
            return order;
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Crea un nuevo pedido basado en el carrito actual del usuario.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var order = await orderService.CreateOrderAsync(userId);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Actualiza el estado de un pedido (Requiere rol Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        try
        {
            await orderService.UpdateOrderStatusAsync(id, dto.Status);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
