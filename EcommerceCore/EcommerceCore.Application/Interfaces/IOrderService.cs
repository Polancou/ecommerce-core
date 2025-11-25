using EcommerceCore.Application.DTOs;
using EcommerceCore.Domain.Models;

namespace EcommerceCore.Application.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync(); // Admin
    Task<OrderDto?> GetOrderByIdAsync(int id, int userId, bool isAdmin);
    Task<OrderDto> CreateOrderAsync(int userId);
    Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
}
