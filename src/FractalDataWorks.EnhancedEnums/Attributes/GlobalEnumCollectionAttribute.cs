using System;
using System.Diagnostics.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Attributes;

/// <summary>
/// Marks an enhanced enum collection for global cross-assembly discovery.
/// When this attribute is present, the source generator will scan all referenced assemblies
/// for enum options, not just the current project.
/// </summary>
/// <remarks>
/// Excluded from code coverage: Attribute class with only constructor and properties for configuration.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
[ExcludeFromCodeCoverage]
public sealed class GlobalEnumCollectionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalEnumCollectionAttribute"/> class.
    /// </summary>
    public GlobalEnumCollectionAttribute()
    {
    }

    /// <summary>
    /// Gets or sets the name of the generated collection class.
    /// If null or empty, uses the base class name with "s" suffix.
    /// </summary>
    public string? CollectionName { get; set; }

    /// <summary>
    /// Gets or sets the default generic return type for generated methods.
    /// If null, uses the base enum type.
    /// </summary>
    public Type? DefaultGenericReturnType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether singleton instances should be used instead of factory methods.
    /// </summary>
    public bool UseSingletonInstances { get; set; }
}
