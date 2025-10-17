namespace FractalDataWorks.MsSql.Sample.Models;

/// <summary>
/// Represents an aggregated order summary.
/// Used for reporting and analytics queries.
/// </summary>
public sealed class OrderSummary
{
    public int OrderId { get; init; }
    public DateTime OrderDate { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public int ItemCount { get; init; }
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
}
