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
public class CartController(ICartService cartService) : ControllerBase
{
    /// <summary>
    /// Obtiene el carrito actual del usuario.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var cart = await cartService.GetCartAsync(userId);
        return Ok(cart);
    }

    /// <summary>
    /// Sincroniza el carrito local (frontend) con el carrito del servidor.
    /// </summary>
    [HttpPost("sync")]
    public async Task<ActionResult<CartDto>> SyncCart([FromBody] List<AddToCartDto> localItems)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var cart = await cartService.SyncCartAsync(userId, localItems);
        return Ok(cart);
    }

    /// <summary>
    /// Agrega un ítem al carrito.
    /// </summary>
    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem([FromBody] AddToCartDto itemDto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var cart = await cartService.AddItemAsync(userId, itemDto);
        return Ok(cart);
    }

    /// <summary>
    /// Actualiza la cantidad de un ítem en el carrito.
    /// </summary>
    [HttpPut("items")]
    public async Task<ActionResult<CartDto>> UpdateItem([FromBody] AddToCartDto itemDto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            var cart = await cartService.UpdateItemQuantityAsync(userId, itemDto.ProductId, itemDto.Quantity);
            return Ok(cart);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Elimina un ítem del carrito.
    /// </summary>
    [HttpDelete("items/{productId}")]
    public async Task<ActionResult<CartDto>> RemoveItem(int productId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            var cart = await cartService.RemoveItemAsync(userId, productId);
            return Ok(cart);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
