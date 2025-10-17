namespace FractalDataWorks.MsSql.Sample.Models;

/// <summary>
/// Represents a product in the inventory system.
/// Maps to inventory.Products table.
/// </summary>
public sealed class Product
{
    public int ProductId { get; init; }
    public int CategoryId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public decimal Cost { get; init; }
    public int QuantityInStock { get; init; }
    public int ReorderLevel { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTime CreatedDate { get; init; }
    public DateTime LastModifiedDate { get; init; }

    public decimal Margin => Price - Cost;
    public decimal MarginPercentage => Cost > 0 ? (Margin / Cost) * 100 : 0;
    public bool NeedsReorder => QuantityInStock <= ReorderLevel;
}
