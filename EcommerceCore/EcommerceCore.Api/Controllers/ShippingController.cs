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
public class ShippingController(IShippingService shippingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ShippingAddressDto>> GetAddress()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var address = await shippingService.GetAddressAsync(userId);
        
        if (address == null) return NoContent();
        
        return Ok(address);
    }

    [HttpPost]
    public async Task<IActionResult> UpsertAddress([FromBody] ShippingAddressDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        await shippingService.UpsertAddressAsync(userId, dto);
        return Ok();
    }
}
