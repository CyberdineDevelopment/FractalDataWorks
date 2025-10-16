using System;

namespace DataConceptDemo.Models;

/// <summary>
/// Represents a PayPal payment response.
/// This is a simplified model of what a PayPal API might return.
/// </summary>
public sealed class PayPalPayment
{
    public string PaymentId { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime CreateTime { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
