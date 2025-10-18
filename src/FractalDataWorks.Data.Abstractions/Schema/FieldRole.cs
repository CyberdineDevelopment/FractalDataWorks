namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Semantic role of a field for query optimization and metadata.
/// </summary>
public enum FieldRole
{
    /// <summary>
    /// Part of unique key, used for joins and lookups.
    /// Example: UserId, OrderId, ProductSku
    /// </summary>
    Identity,

    /// <summary>
    /// Descriptive/dimensional data for filtering and grouping.
    /// Example: CustomerName, OrderDate, ProductCategory, Email
    /// </summary>
    Attribute,

    /// <summary>
    /// Numeric/aggregatable facts for SUM, AVG, COUNT operations.
    /// Example: TotalAmount, Quantity, Revenue, LoginCount
    /// </summary>
    Measure
}
