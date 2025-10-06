using System;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Collections;

/// <summary>
/// Base class for type option types that enforces Id, Name, and Category properties through constructor initialization.
/// This base class uses auto properties instead of abstract properties for cleaner code.
/// </summary>
/// <typeparam name="T">The derived type option type.</typeparam>
public abstract class TypeOptionBase<T> : ITypeOption<T> where T : ITypeOption<T>
{
    /// <summary>
    /// Gets the unique identifier for this type option value.
    /// </summary>
    [TypeLookup("Id")]
    public int Id { get; }

    /// <summary>
    /// Gets the name of this type option value.
    /// </summary>
    [TypeLookup("Name")]
    public string Name { get; }

    /// <summary>
    /// Backing field for the category value.
    /// </summary>
    private readonly string _category;

    /// <summary>
    /// Gets the category of this type option value.
    /// </summary>
    public string Category => string.IsNullOrEmpty(_category) ? "NotCategorized" : _category;

    /// <summary>
    /// Gets the configuration key for this type option value.
    /// Used for configuration lookups and service registration.
    /// </summary>
    public string ConfigurationKey { get; }

    /// <summary>
    /// Gets the display name for this type option value.
    /// Used for user-facing displays and documentation.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the description of this type option value.
    /// Provides detailed information about what this option does.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeOptionBase{T}"/> class with default category.
    /// </summary>
    /// <param name="id">The unique identifier for this type option value.</param>
    /// <param name="name">The name of this type option value.</param>
    /// <exception cref="ArgumentNullException">Thrown when name is null.</exception>
    protected TypeOptionBase(int id, string name) : this(id, name, string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeOptionBase{T}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this type option value.</param>
    /// <param name="name">The name of this type option value.</param>
    /// <param name="category">The category of this type option value. Pass null or empty string for default "NotCategorized".</param>
    /// <exception cref="ArgumentNullException">Thrown when name is null.</exception>
    protected TypeOptionBase(int id, string name, string? category)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));

        Id = id;
        Name = name;
        _category = category ?? string.Empty;
        ConfigurationKey = $"TypeOptions:{name}";
        DisplayName = name;
        Description = $"Type option: {name}";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeOptionBase{T}"/> class with full metadata.
    /// </summary>
    /// <param name="id">The unique identifier for this type option value.</param>
    /// <param name="name">The name of this type option value.</param>
    /// <param name="configurationKey">The configuration key for service registration and lookups.</param>
    /// <param name="displayName">The display name for user-facing representations.</param>
    /// <param name="description">The detailed description of this type option.</param>
    /// <param name="category">The category of this type option value.</param>
    /// <exception cref="ArgumentNullException">Thrown when name is null.</exception>
    protected TypeOptionBase(int id, string name, string configurationKey, string displayName, string description, string? category)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));

        Id = id;
        Name = name;
        _category = category ?? string.Empty;
        ConfigurationKey = configurationKey ?? $"TypeOptions:{name}";
        DisplayName = displayName ?? name;
        Description = description ?? $"Type option: {name}";
    }
}