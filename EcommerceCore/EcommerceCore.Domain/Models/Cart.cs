using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceCore.Domain.Models;

/// <summary>
/// Representa el carrito de compras de un usuario.
/// </summary>
public class Cart
{
    [Key] public int Id { get; private set; }

    [Required] public int UserId { get; private set; }
    public Usuario User { get; private set; }

    public List<CartItem> Items { get; private set; } = new();

    /// <summary>
    /// Fecha de la última actualización del carrito.
    /// </summary>
    public DateTime LastUpdated { get; private set; }

    private Cart()
    {
    }

    public Cart(int userId)
    {
        UserId = userId;
        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Agrega un producto al carrito o actualiza su cantidad si ya existe.
    /// </summary>
    public void AddItem(int productId, int quantity)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            Items.Add(new CartItem(productId, quantity));
        }

        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Elimina un producto del carrito.
    /// </summary>
    public void RemoveItem(int productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
        }

        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Vacía el carrito por completo.
    /// </summary>
    public void Clear()
    {
        Items.Clear();
        LastUpdated = DateTime.UtcNow;
    }
}

/// <summary>
/// Representa un ítem dentro del carrito de compras.
/// </summary>
public class CartItem
{
    [Key] public int Id { get; private set; }

    public int CartId { get; private set; }
    public Cart Cart { get; private set; }

    public int ProductId { get; private set; }
    public Product Product { get; private set; }

    [Required] public int Quantity { get; private set; }

    private CartItem()
    {
    }

    public CartItem(int productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }

    public void UpdateQuantity(int quantity)
    {
        Quantity = quantity;
    }
}
