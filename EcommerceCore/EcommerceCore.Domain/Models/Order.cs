using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceCore.Domain.Models;

public enum OrderStatus
{
    Pending,
    Paid,
    Shipped,
    Delivered,
    Cancelled
}

/// <summary>
/// Representa un pedido realizado por un usuario.
/// </summary>
public class Order
{
    [Key] public int Id { get; private set; }

    [Required] public int UserId { get; private set; }
    public Usuario User { get; private set; }

    /// <summary>
    /// Fecha en la que se realizó el pedido.
    /// </summary>
    public DateTime OrderDate { get; private set; }

    /// <summary>
    /// Monto total del pedido.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Estado actual del pedido.
    /// </summary>
    [Required]
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// ID del intento de pago (Stripe).
    /// </summary>
    public string? PaymentIntentId { get; private set; }

    public List<OrderItem> Items { get; private set; } = new();

    private Order()
    {
    }

    public Order(int userId, decimal totalAmount)
    {
        UserId = userId;
        TotalAmount = totalAmount;
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;
    }

    public void AddItem(OrderItem item)
    {
        Items.Add(item);
    }

    public void SetPaymentIntentId(string paymentIntentId)
    {
        PaymentIntentId = paymentIntentId;
    }

    public void MarkAsPaid()
    {
        Status = OrderStatus.Paid;
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
    }
}

/// <summary>
/// Representa un ítem dentro de un pedido.
/// </summary>
public class OrderItem
{
    [Key] public int Id { get; private set; }

    public int OrderId { get; private set; }
    public Order Order { get; private set; }

    public int ProductId { get; private set; }
    public Product Product { get; private set; }

    [Required] public string ProductName { get; private set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; private set; }

    [Required] public int Quantity { get; private set; }

    private OrderItem()
    {
    }

    public OrderItem(int productId, string productName, decimal unitPrice, int quantity)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}
