using EcommerceCore.Application.DTOs;

namespace EcommerceCore.Application.Interfaces;

public interface IShippingService
{
    Task<ShippingAddressDto?> GetAddressAsync(int userId);
    Task UpsertAddressAsync(int userId, ShippingAddressDto dto);
}
