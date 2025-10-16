using System;

namespace DataConceptDemo.Models;

/// <summary>
/// Unified transaction model.
/// Multiple payment sources (PayPal, Stripe, SQL) will be transformed to this common format.
/// </summary>
public sealed class Transaction
{
    /// <summary>
    /// Gets or sets the transaction identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transaction amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the transaction date.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the payment method.
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transaction status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
