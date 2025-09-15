using System;

namespace FractalDataWorks.Collections.Attributes;

/// <summary>
/// Marks a concrete type option with a method name.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class TypeOptionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeOptionAttribute"/> class.
    /// Uses the class name as the method name.
    /// </summary>
    public TypeOptionAttribute()
    {
        Name = null; // Will be inferred from class name
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeOptionAttribute"/> class.
    /// </summary>
    /// <param name="name">The name for the method/property in the generated collection.</param>
    public TypeOptionAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Gets the name for the method/property in the generated collection.
    /// If null, the class name will be used.
    /// </summary>
    public string? Name { get; }
}