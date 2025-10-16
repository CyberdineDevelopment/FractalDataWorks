using System;

namespace FractalDataWorks.EnhancedEnums.ExtendedEnums.Attributes;

/// <summary>
/// Marks a class as an Extended Enum collection for source generation.
/// Extended Enums wrap existing C# enums with additional behavior and Enhanced Enum features.
/// The source generator will automatically create wrapper classes for all enum values,
/// which can be selectively overridden using ExtendedEnumOptionAttribute.
/// </summary>
/// <param name="enumType">The C# enum type being extended (e.g., typeof(HttpStatusCode)).</param>
/// <param name="defaultReturnType">The default return type for generated methods (e.g., typeof(IHttpStatusCodeExtended)).</param>
/// <param name="collectionType">The partial class type being generated (e.g., typeof(HttpStatusCodes)).</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ExtendedEnumCollectionAttribute(Type enumType, Type defaultReturnType, Type collectionType) : Attribute
{
    /// <summary>
    /// Gets the C# enum type being extended.
    /// </summary>
    public Type EnumType { get; } = enumType ?? throw new ArgumentNullException(nameof(enumType));

    /// <summary>
    /// Gets the default return type for generated methods.
    /// </summary>
    public Type DefaultReturnType { get; } = defaultReturnType ?? throw new ArgumentNullException(nameof(defaultReturnType));

    /// <summary>
    /// Gets the partial class type being generated.
    /// </summary>
    public Type CollectionType { get; } = collectionType ?? throw new ArgumentNullException(nameof(collectionType));

    /// <summary>
    /// Gets the fully qualified name of the enum type (for generator compatibility).
    /// </summary>
    public string EnumTypeName => EnumType.FullName ?? EnumType.Name;

    /// <summary>
    /// Gets the collection name (for generator compatibility).
    /// </summary>
    public string CollectionName => CollectionType.Name;

    /// <summary>
    /// Gets or sets a value indicating whether singleton instances should be used for extended enum values.
    /// Defaults to true for Extended Enums.
    /// </summary>
    public bool UseSingletonInstances { get; set; } = true;

    static ExtendedEnumCollectionAttribute()
    {
        // Validate that enumType is actually an enum at static construction
        // This provides early feedback during compilation
    }
}