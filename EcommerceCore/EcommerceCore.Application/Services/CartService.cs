using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using EcommerceCore.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCore.Application.Services;

public class CartService(IApplicationDbContext context) : ICartService
{
    public async Task<CartDto> GetCartAsync(int userId)
    {
        var cart = await GetCartEntityAsync(userId);
        if (cart == null) return new CartDto { Id = 0, Items = new List<CartItemDto>() };

        return MapToDto(cart);
    }

    public async Task<CartDto> AddItemAsync(int userId, AddToCartDto dto)
    {
        var cart = await GetCartEntityAsync(userId);
        if (cart == null)
        {
            cart = new Cart(userId);
            context.Carts.Add(cart);
        }

        cart.AddItem(dto.ProductId, dto.Quantity);
        await context.SaveChangesAsync();

        // Refetch to ensure product details are loaded for DTO
        cart = await GetCartEntityAsync(userId);
        return MapToDto(cart!);
    }

    public async Task<CartDto> UpdateItemQuantityAsync(int userId, int productId, int quantity)
    {
        var cart = await GetCartEntityAsync(userId);
        if (cart == null) throw new KeyNotFoundException("Carrito no encontrado.");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            item.UpdateQuantity(quantity);
            await context.SaveChangesAsync();
        }

        // Refetch
        cart = await GetCartEntityAsync(userId);
        return MapToDto(cart!);
    }

    public async Task<CartDto> RemoveItemAsync(int userId, int productId)
    {
        var cart = await GetCartEntityAsync(userId);
        if (cart == null) throw new KeyNotFoundException("Carrito no encontrado.");

        cart.RemoveItem(productId);
        await context.SaveChangesAsync();

        // Refetch to ensure correct state
        cart = await GetCartEntityAsync(userId);
        return MapToDto(cart!);
    }

    public async Task<CartDto> SyncCartAsync(int userId, List<AddToCartDto> localItems)
    {
        var cart = await GetCartEntityAsync(userId);
        if (cart == null)
        {
            cart = new Cart(userId);
            context.Carts.Add(cart);
        }

        foreach (var item in localItems)
        {
            cart.AddItem(item.ProductId, item.Quantity);
        }

        await context.SaveChangesAsync();

        // Refetch
        cart = await GetCartEntityAsync(userId);
        return MapToDto(cart!);
    }

    public async Task ClearCartAsync(int userId)
    {
        var cart = await GetCartEntityAsync(userId);
        if (cart != null)
        {
            cart.Clear();
            await context.SaveChangesAsync();
        }
    }

    private async Task<Cart?> GetCartEntityAsync(int userId)
    {
        return await context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    private static CartDto MapToDto(Cart cart)
    {
        return new CartDto
        {
            Id = cart.Id,
            Items = cart.Items.Select(i => new CartItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Price = i.Product.Price,
                Quantity = i.Quantity,
                ImageUrl = i.Product.ImageUrl
            }).ToList()
        };
    }
}
