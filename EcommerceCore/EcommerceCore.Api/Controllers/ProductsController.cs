using Asp.Versioning;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCore.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ProductsController(IProductService productService) : ControllerBase
{
    /// <summary>
    /// Obtiene la lista completa de productos.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ProductDto>>> GetProducts([FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var products = await productService.GetAllAsync(search, page, pageSize);
        return Ok(products);
    }

    /// <summary>
    /// Obtiene un producto específico por su ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await productService.GetByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return product;
    }

    /// <summary>
    /// Crea un nuevo producto (Requiere rol Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto productDto)
    {
        var createdProduct = await productService.CreateAsync(productDto);
        return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
    }

    /// <summary>
    /// Actualiza un producto existente (Requiere rol Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto productDto)
    {
        try
        {
            await productService.UpdateAsync(id, productDto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Elimina un producto (Requiere rol Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            await productService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Sube una imagen para un producto (Requiere rol Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/image")]
    public async Task<IActionResult> UploadProductImage(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No se ha proporcionado ningún archivo." });

        if (file.Length > 1024 * 1024 * 5) // 5MB limit
            return BadRequest(new { message = "El archivo debe ser menor a 5 MB." });

        try
        {
            var imageUrl = await productService.UploadImageAsync(id, file);
            return Ok(new { imageUrl });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
