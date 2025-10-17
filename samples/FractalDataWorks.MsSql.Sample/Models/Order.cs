namespace FractalDataWorks.MsSql.Sample.Models;

/// <summary>
/// Represents a sales order.
/// Maps to sales.Orders table.
/// </summary>
public sealed class Order
{
    public int OrderId { get; init; }
    public int CustomerId { get; init; }
    public DateTime OrderDate { get; init; }
    public DateTime? RequiredDate { get; init; }
    public DateTime? ShippedDate { get; init; }
    public string Status { get; init; } = "Pending";
    public decimal SubTotal { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal ShippingCost { get; init; }
    public decimal TotalAmount { get; init; }
    public string? PaymentMethod { get; init; }
    public string PaymentStatus { get; init; } = "Pending";
    public string? ShippingAddress { get; init; }
    public string? BillingAddress { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime LastModifiedDate { get; init; }

    public bool IsCompleted => Status == "Delivered";
    public bool IsCancelled => Status == "Cancelled";
    public bool IsPaid => PaymentStatus == "Paid";
}
