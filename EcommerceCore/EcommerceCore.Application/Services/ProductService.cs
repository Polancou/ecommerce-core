using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using EcommerceCore.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace EcommerceCore.Application.Services;

public class ProductService(IApplicationDbContext context, IFileStorageService fileStorageService) : IProductService
{
    public async Task<PaginatedResult<ProductDto>> GetAllAsync(string? searchTerm = null, int page = 1,
        int pageSize = 10)
    {
        var query = context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl,
                Category = p.Category
            })
            .ToListAsync();

        return new PaginatedResult<ProductDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var p = await context.Products.FindAsync(id);
        if (p == null) return null;

        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            ImageUrl = p.ImageUrl,
            Category = p.Category
        };
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product(dto.Name, dto.Description, dto.Price, dto.Stock, dto.ImageUrl, dto.Category);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            Category = product.Category
        };
    }

    public async Task UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await context.Products.FindAsync(id);
        if (product == null) throw new KeyNotFoundException("Producto no encontrado.");

        product.UpdateDetails(dto.Name, dto.Description, dto.Price, dto.ImageUrl, dto.Category);

        // Calcular diferencia de stock para actualizar
        var stockDiff = dto.Stock - product.Stock;
        if (stockDiff != 0)
        {
            product.UpdateStock(stockDiff);
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product == null) throw new KeyNotFoundException("Producto no encontrado.");

        // Eliminar imagen si existe
        if (!string.IsNullOrEmpty(product.ImageUrl) && !product.ImageUrl.StartsWith("http"))
        {
            await fileStorageService.DeleteFileAsync(product.ImageUrl);
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync();
    }

    public async Task<string> UploadImageAsync(int id, IFormFile file)
    {
        var product = await context.Products.FindAsync(id);
        if (product == null) throw new KeyNotFoundException("Producto no encontrado.");

        // Borrar imagen anterior si no es una URL externa (placeholder)
        if (!string.IsNullOrEmpty(product.ImageUrl) && !product.ImageUrl.StartsWith("http"))
        {
            await fileStorageService.DeleteFileAsync(product.ImageUrl);
        }

        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"products/{Guid.NewGuid()}{fileExtension}";

        string fileUrl;
        await using (var stream = file.OpenReadStream())
        {
            fileUrl = await fileStorageService.SaveFileAsync(stream, uniqueFileName);
        }

        product.UpdateDetails(product.Name, product.Description, product.Price, fileUrl, product.Category);
        await context.SaveChangesAsync();

        return fileUrl;
    }
}
