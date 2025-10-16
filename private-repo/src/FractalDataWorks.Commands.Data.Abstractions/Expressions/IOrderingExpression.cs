using System.Collections.Generic;

namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Interface for ordering expressions (ORDER BY clause representation).
/// </summary>
/// <remarks>
/// Represents which fields to sort by and in which direction.
/// Translators convert this to SQL ORDER BY, OData $orderby, sorting logic, etc.
/// </remarks>
public interface IOrderingExpression
{
    /// <summary>
    /// Gets the ordered fields.
    /// </summary>
    /// <value>A collection of fields to sort by, in order of precedence.</value>
    IReadOnlyList<OrderedField> OrderedFields { get; }
}
