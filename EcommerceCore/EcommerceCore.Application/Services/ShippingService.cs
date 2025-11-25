using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using EcommerceCore.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCore.Application.Services;

public class ShippingService(IApplicationDbContext context) : IShippingService
{
    public async Task<ShippingAddressDto?> GetAddressAsync(int userId)
    {
        var address = await context.ShippingAddresses
            .FirstOrDefaultAsync(a => a.UserId == userId);

        if (address == null) return null;

        return new ShippingAddressDto
        {
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country
        };
    }

    public async Task UpsertAddressAsync(int userId, ShippingAddressDto dto)
    {
        var address = await context.ShippingAddresses
            .FirstOrDefaultAsync(a => a.UserId == userId);

        if (address == null)
        {
            address = new ShippingAddress
            {
                UserId = userId,
                AddressLine1 = dto.AddressLine1,
                AddressLine2 = dto.AddressLine2,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Country = dto.Country
            };
            context.ShippingAddresses.Add(address);
        }
        else
        {
            address.AddressLine1 = dto.AddressLine1;
            address.AddressLine2 = dto.AddressLine2;
            address.City = dto.City;
            address.State = dto.State;
            address.PostalCode = dto.PostalCode;
            address.Country = dto.Country;
        }

        await context.SaveChangesAsync();
    }
}
