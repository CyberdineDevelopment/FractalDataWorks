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
    [TypeLookup("GetById")]
    public int Id { get; }

    /// <summary>
    /// Gets the name of this type option value.
    /// </summary>
    [TypeLookup("GetByName")]
    public string Name { get; }

    /// <summary>
    /// Backing field for the category value.
    /// </summary>
    private readonly string _category;

    /// <summary>
    /// Gets the category of this type option value.
    /// </summary>
    [TypeLookup("GetByCategory")]
    public string Category => string.IsNullOrEmpty(_category) ? "NotCategorized" : _category;

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
    }
}