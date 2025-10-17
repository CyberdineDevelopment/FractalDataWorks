namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Represents a single projected field (column/property to select).
/// </summary>
public sealed record ProjectionField
{
    /// <summary>
    /// Gets the property name to project.
    /// </summary>
    public required string PropertyName { get; init; }

    /// <summary>
    /// Gets the alias for this field (optional).
    /// </summary>
    public string? Alias { get; init; }
}
