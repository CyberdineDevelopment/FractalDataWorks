namespace FractalDataWorks.EnhancedEnums;

/// <summary>
/// Represents a type option that provides additional functionality for type collections.
/// This interface enables strongly-typed type options with identifiers, names, categories, and the ability to represent an empty state.
/// </summary>
/// <typeparam name="T">The implementing type, used for self-referencing generics pattern.</typeparam>
public interface ITypeOption<T> : ITypeOption
    where T : ITypeOption<T>
{

}

/// <summary>
/// Represents a type option that provides additional functionality for type collections.
/// This interface enables strongly-typed type options with identifiers, names, categories, and the ability to represent an empty state.
/// </summary>
public interface ITypeOption
{
    /// <summary>
    /// Gets the unique identifier for this type option value.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the display name or string representation of this type option value.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the category for this type option value.
    /// </summary>
    public string Category { get; }
}