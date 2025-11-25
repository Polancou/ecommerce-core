using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceCore.Domain.Models;

public class ShippingAddress
{
    [Key] public int Id { get; set; }

    [Required] public int UserId { get; set; }

    [ForeignKey("UserId")] public Usuario User { get; set; } = null!;

    [Required] [MaxLength(100)] public string AddressLine1 { get; set; } = string.Empty;

    [MaxLength(100)] public string? AddressLine2 { get; set; }

    [Required] [MaxLength(50)] public string City { get; set; } = string.Empty;

    [Required] [MaxLength(50)] public string State { get; set; } = string.Empty;

    [Required] [MaxLength(20)] public string PostalCode { get; set; } = string.Empty;

    [Required] [MaxLength(50)] public string Country { get; set; } = string.Empty;
}
