using AutoMapper;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using EcommerceCore.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCore.Application.Services;

public class OrderService(IApplicationDbContext context, IMapper mapper) : IOrderService
{
    /// <summary>
    /// Obtiene todos los pedidos de un usuario específico.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <returns>Una colección de DTOs de pedidos del usuario.</returns>
    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId)
    {
        // Busca los pedidos del usuario, incluyendo los ítems de cada pedido.
        var orders = await context.Orders
            .Include(navigationPropertyPath: o => o.Items)
            .Where(predicate: o => o.UserId == userId)
            .ToListAsync();

        // Mapea la lista de entidades Order a DTOs de Order.
        return mapper.Map<IEnumerable<OrderDto>>(source: orders);
    }

    /// <summary>
    /// Obtiene todos los pedidos del sistema.
    /// </summary>
    /// <returns>Una colección de DTOs de todos los pedidos.</returns>
    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        // Busca todos los pedidos, incluyendo los ítems de cada pedido.
        var orders = await context.Orders
            .Include(navigationPropertyPath: o => o.Items)
            .ToListAsync();

        // Mapea la lista de entidades Order a DTOs de Order.
        return mapper.Map<IEnumerable<OrderDto>>(source: orders);
    }

    /// <summary>
    /// Obtiene un pedido específico por su ID.
    /// </summary>
    /// <param name="id">El ID del pedido.</param>
    /// <param name="userId">El ID del usuario que intenta acceder al pedido.</param>
    /// <param name="isAdmin">Indica si el usuario es un administrador.</param>
    /// <returns>El DTO del pedido si se encuentra y el usuario tiene permiso, de lo contrario, null.</returns>
    /// <exception cref="UnauthorizedAccessException">Se lanza si el usuario no tiene permiso para ver el pedido.</exception>
    public async Task<OrderDto?> GetOrderByIdAsync(int id, int userId, bool isAdmin)
    {
        // Busca el pedido por ID, incluyendo sus ítems.
        var order = await context.Orders
            .Include(navigationPropertyPath: o => o.Items)
            .FirstOrDefaultAsync(predicate: o => o.Id == id);

        // Si el pedido no existe, retorna null.
        if (order == null) return null;
        // Verifica si el usuario tiene permiso para ver el pedido (debe ser el propietario o un administrador).
        if (order.UserId != userId && !isAdmin)
            throw new UnauthorizedAccessException(message: "No tienes permiso para ver este pedido.");

        // Mapea la entidad Order a un DTO de Order.
        return mapper.Map<OrderDto>(source: order);
    }

    /// <summary>
    /// Crea un nuevo pedido a partir del carrito de compras de un usuario.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <param name="shippingAddress">La dirección de envío para el pedido.</param>
    /// <returns>El DTO del pedido creado.</returns>
    /// <exception cref="InvalidOperationException">Se lanza si el carrito del usuario está vacío.</exception>
    public async Task<OrderDto> CreateOrderAsync(int userId, ShippingAddressDto shippingAddress)
    {
        // Obtiene el carrito del usuario, incluyendo sus ítems y los productos asociados.
        var cart = await context.Carts
            .Include(navigationPropertyPath: c => c.Items)
            .ThenInclude(navigationPropertyPath: i => i.Product)
            .FirstOrDefaultAsync(predicate: c => c.UserId == userId);

        // Si el carrito está vacío o no existe, lanza una excepción.
        if (cart == null || !cart.Items.Any())
        {
            throw new InvalidOperationException(message: "El carrito está vacío.");
        }

        // Crea una nueva instancia de Order con el ID del usuario y el total del carrito.
        var order = new Order(userId: userId,
            totalAmount: cart.Items.Sum(selector: i => i.Quantity * i.Product.Price));

        // Establece la dirección de envío del pedido.
        order.SetShippingAddress(
            addressLine1: shippingAddress.AddressLine1,
            addressLine2: shippingAddress.AddressLine2,
            city: shippingAddress.City,
            state: shippingAddress.State,
            postalCode: shippingAddress.PostalCode,
            country: shippingAddress.Country
        );

        // Agrega los ítems del carrito al pedido y actualiza el stock de los productos.
        foreach (var item in cart.Items)
        {
            order.AddItem(item: new OrderItem(productId: item.ProductId,
                productName: item.Product.Name,
                unitPrice: item.Product.Price,
                quantity: item.Quantity));

            // Actualiza el stock del producto.
            item.Product.UpdateStock(quantity: item.Product.Stock - item.Quantity);
        }

        // Agrega el nuevo pedido al contexto y elimina el carrito.
        context.Orders.Add(entity: order);
        context.Carts.Remove(entity: cart); // Vacía el carrito después de crear el pedido.

        // Guarda los cambios en la base de datos.
        await context.SaveChangesAsync();

        // Mapea la entidad Order a un DTO de Order.
        return mapper.Map<OrderDto>(source: order);
    }

    /// <summary>
    /// Actualiza el estado de un pedido existente.
    /// </summary>
    /// <param name="orderId">El ID del pedido a actualizar.</param>
    /// <param name="newStatus">El nuevo estado del pedido.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    /// <exception cref="KeyNotFoundException">Se lanza si el pedido no se encuentra.</exception>
    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
    {
        // Busca el pedido por su ID.
        var order = await context.Orders.FindAsync(keyValues: orderId);
        // Si el pedido no existe, lanza una excepción.
        if (order == null) throw new KeyNotFoundException(message: "Pedido no encontrado.");

        // Actualiza el estado del pedido.
        order.UpdateStatus(newStatus: newStatus);
        // Guarda los cambios en la base de datos.
        await context.SaveChangesAsync();
    }
}
