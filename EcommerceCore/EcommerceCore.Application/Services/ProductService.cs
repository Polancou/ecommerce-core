using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using EcommerceCore.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCore.Application.Services;

public class ProductService(IApplicationDbContext context) : IProductService
{
    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        return await context.Products
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

        context.Products.Remove(product);
        await context.SaveChangesAsync();
    }
}
