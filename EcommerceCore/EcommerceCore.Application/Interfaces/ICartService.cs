using EcommerceCore.Application.DTOs;

namespace EcommerceCore.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);
    Task<CartDto> AddItemAsync(int userId, AddToCartDto dto);
    Task<CartDto> UpdateItemQuantityAsync(int userId, int productId, int quantity);
    Task<CartDto> RemoveItemAsync(int userId, int productId);
    Task<CartDto> SyncCartAsync(int userId, List<AddToCartDto> localItems);
    Task ClearCartAsync(int userId);
}
