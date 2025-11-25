using EcommerceCore.Application.DTOs;

namespace EcommerceCore.Application.Interfaces;

public interface IPaymentService
{
    Task<string> CreatePaymentIntentAsync(decimal amount, string currency = "usd");
}
