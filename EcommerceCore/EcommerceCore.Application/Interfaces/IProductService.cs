using EcommerceCore.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace EcommerceCore.Application.Interfaces;

public interface IProductService
{
    /// <summary>
    /// Obtiene todos los productos con paginación y un término de búsqueda opcional.
    /// </summary>
    /// <param name="searchTerm">Término de búsqueda opcional para filtrar productos.</param>
    /// <param name="page">Número de página actual.</param>
    /// <param name="pageSize">Cantidad de elementos por página.</param>
    /// <returns>Un resultado paginado de objetos ProductDto.</returns>
    Task<PaginatedResult<ProductDto>> GetAllAsync(string? searchTerm = null, int page = 1, int pageSize = 10);

    /// <summary>
    /// Obtiene un producto por su identificador único.
    /// </summary>
    /// <param name="id">El ID del producto a buscar.</param>
    /// <returns>Un objeto ProductDto si se encuentra, de lo contrario, null.</returns>
    Task<ProductDto?> GetByIdAsync(int id);

    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    /// <param name="dto">Los datos para crear el producto.</param>
    /// <returns>El objeto ProductDto del producto recién creado.</returns>
    Task<ProductDto> CreateAsync(CreateProductDto dto);

    /// <summary>
    /// Actualiza un producto existente.
    /// </summary>
    /// <param name="id">El ID del producto a actualizar.</param>
    /// <param name="dto">Los datos actualizados del producto.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    Task UpdateAsync(int id, UpdateProductDto dto);

    /// <summary>
    /// Elimina un producto por su identificador único.
    /// </summary>
    /// <param name="id">El ID del producto a eliminar.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    Task DeleteAsync(int id);

    /// <summary>
    /// Sube una imagen para un producto específico.
    /// </summary>
    /// <param name="id">El ID del producto al que se le asociará la imagen.</param>
    /// <param name="file">El archivo de imagen a subir.</param>
    /// <returns>La URL o el nombre del archivo de la imagen subida.</returns>
    Task<string> UploadImageAsync(int id, IFormFile file);
}
