namespace FractalDataWorks.MsSql.Sample.Models;

/// <summary>
/// Represents a product category.
/// Maps to inventory.Categories table.
/// </summary>
public sealed class Category
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTime CreatedDate { get; init; }
}
