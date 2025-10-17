namespace FractalDataWorks.MsSql.Sample.Models;

/// <summary>
/// Represents a line item in a sales order.
/// Maps to sales.OrderItems table.
/// </summary>
public sealed class OrderItem
{
    public int OrderItemId { get; init; }
    public int OrderId { get; init; }
    public int ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Discount { get; init; }
    public decimal LineTotal { get; init; }

    public decimal DiscountAmount => (UnitPrice * Quantity) * (Discount / 100);
    public decimal SubTotal => (UnitPrice * Quantity) - DiscountAmount;
}
