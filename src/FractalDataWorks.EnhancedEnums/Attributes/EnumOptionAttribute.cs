using System;

namespace FractalDataWorks.EnhancedEnums.Attributes;

/// <summary>
/// Marks a concrete enum option with a method name.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class EnumOptionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumOptionAttribute"/> class.
    /// Uses the class name as the method name.
    /// </summary>
    public EnumOptionAttribute()
    {
        Name = null; // Will be inferred from class name
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumOptionAttribute"/> class.
    /// </summary>
    /// <param name="name">The name for the method/property in the generated collection.</param>
    public EnumOptionAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Gets the name for the method/property in the generated collection.
    /// If null, the class name will be used.
    /// </summary>
    public string? Name { get; }
}
