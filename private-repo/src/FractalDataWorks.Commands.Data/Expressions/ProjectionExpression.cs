using System.Collections.Generic;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Implementation of IProjectionExpression for SELECT clause representation.
/// </summary>
public sealed class ProjectionExpression : IProjectionExpression
{
    /// <summary>
    /// Gets or sets the fields to project.
    /// Empty list means select all fields.
    /// </summary>
    public required IReadOnlyList<ProjectionField> Fields { get; init; }
}
