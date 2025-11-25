using System.ComponentModel.DataAnnotations;

namespace EcommerceCore.Application.DTOs;

public class ShippingAddressDto
{
    [Required]
    [MaxLength(100)]
    public string AddressLine1 { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? AddressLine2 { get; set; }

    [Required]
    [MaxLength(50)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string State { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Country { get; set; } = string.Empty;
}
