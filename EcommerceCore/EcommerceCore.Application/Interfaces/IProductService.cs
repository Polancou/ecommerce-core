using EcommerceCore.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace EcommerceCore.Application.Interfaces;

public interface IProductService
{
    Task<PaginatedResult<ProductDto>> GetAllAsync(string? searchTerm = null, int page = 1, int pageSize = 10);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task UpdateAsync(int id, UpdateProductDto dto);
    Task DeleteAsync(int id);
    Task<string> UploadImageAsync(int id, IFormFile file);
}
