namespace FractalDataWorks.Commands.Abstractions.Commands;

/// <summary>
/// Specifies ordering for query results.
/// </summary>
public interface IOrderSpecification
{
    /// <summary>
    /// Gets the field name to order by.
    /// </summary>
    string FieldName { get; }

    /// <summary>
    /// Gets whether to order in descending order.
    /// </summary>
    bool IsDescending { get; }
}
