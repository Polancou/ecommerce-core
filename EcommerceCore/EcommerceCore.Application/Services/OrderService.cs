using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using EcommerceCore.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCore.Application.Services;

public class OrderService(IApplicationDbContext context) : IOrderService
{
    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId)
    {
        return await context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .Select(o => MapToDto(o))
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        return await context.Orders
            .Include(o => o.Items)
            .Select(o => MapToDto(o))
            .ToListAsync();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id, int userId, bool isAdmin)
    {
        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return null;
        if (order.UserId != userId && !isAdmin) throw new UnauthorizedAccessException("No tienes permiso para ver este pedido.");

        return MapToDto(order);
    }

    public async Task<OrderDto> CreateOrderAsync(int userId)
    {
        var cart = await context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            throw new InvalidOperationException("El carrito está vacío.");
        }

        var totalAmount = cart.Items.Sum(i => i.Product.Price * i.Quantity);
        var order = new Order(userId, totalAmount);

        foreach (var cartItem in cart.Items)
        {
            var orderItem = new OrderItem(cartItem.ProductId, cartItem.Product.Name, cartItem.Product.Price, cartItem.Quantity);
            order.AddItem(orderItem);
            
            // Reducir stock
            cartItem.Product.UpdateStock(-cartItem.Quantity);
        }

        context.Orders.Add(order);
        context.Carts.Remove(cart); // Limpiar carrito

        await context.SaveChangesAsync();

        return MapToDto(order);
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList()
        };
    }
}
