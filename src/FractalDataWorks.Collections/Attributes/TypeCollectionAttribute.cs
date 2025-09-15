using System;

namespace FractalDataWorks.Collections.Attributes;

/// <summary>
/// Marks a class as a type collection base that should have a collection generated.
/// The source generator will create a static collection class for all types that inherit from this base.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class TypeCollectionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeCollectionAttribute"/> class.
    /// </summary>
    public TypeCollectionAttribute()
    {
    }

    /// <summary>
    /// Gets or sets the name of the generated collection class.
    /// If null or empty, uses the base class name with "s" suffix.
    /// </summary>
    public string? CollectionName { get; set; }

    /// <summary>
    /// Gets or sets the default generic return type for generated methods.
    /// If null, uses the base type.
    /// </summary>
    public Type? DefaultGenericReturnType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether singleton instances should be used instead of factory methods.
    /// </summary>
    public bool UseSingletonInstances { get; set; }
}