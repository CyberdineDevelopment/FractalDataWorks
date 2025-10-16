using System.Collections.Generic;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Implementation of IOrderingExpression for ORDER BY clause representation.
/// </summary>
public sealed class OrderingExpression : IOrderingExpression
{
    /// <summary>
    /// Gets or sets the ordered fields.
    /// Order in the list determines sort precedence.
    /// </summary>
    public required IReadOnlyList<OrderedField> OrderedFields { get; init; }
}
