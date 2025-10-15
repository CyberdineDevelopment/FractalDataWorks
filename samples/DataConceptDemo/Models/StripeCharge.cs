using System;

namespace DataConceptDemo.Models;

/// <summary>
/// Represents a Stripe charge response.
/// This is a simplified model of what a Stripe API might return.
/// </summary>
public sealed class StripeCharge
{
    public string Id { get; set; } = string.Empty;
    public long Amount { get; set; } // Stripe uses cents
    public string Currency { get; set; } = "usd";
    public long Created { get; set; } // Unix timestamp
    public string PaymentMethodType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
