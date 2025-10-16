namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Represents a single ordered field (property and direction).
/// </summary>
public sealed record OrderedField
{
    /// <summary>
    /// Gets the property name to order by.
    /// </summary>
    public required string PropertyName { get; init; }

    /// <summary>
    /// Gets the sort direction.
    /// This is a SortDirection EnhancedEnum, not a traditional enum!
    /// </summary>
    public required SortDirection Direction { get; init; }
}
