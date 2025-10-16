using System;

namespace FractalDataWorks.EnhancedEnums.Attributes;

/// <summary>
/// Marks a class as a static enum collection that always inherits base class methods.
/// The source generator will create a static class with all discovered enum options
/// and automatically reconstruct all methods from the base collection class.
/// </summary>
/// <remarks>
/// This attribute is for local discovery within the current compilation only.
/// Unlike EnumCollection, this always inherits base methods without requiring InheritsBaseMethods.
/// For cross-assembly discovery, use GlobalStaticEnumCollectionAttribute instead.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class StaticEnumCollectionAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the generated static collection class.
    /// </summary>
    public string CollectionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default generic return type for reconstructed methods.
    /// When set, all reconstructed methods that return the enum type will instead return this type.
    /// This is useful when you want methods to return an interface type instead of the concrete type.
    /// </summary>
    public Type? DefaultGenericReturnType { get; set; }

    /// <summary>
    /// Gets or sets whether to use singleton instances for enum options.
    /// When true, each enum option will be created once and cached.
    /// When false, new instances are created on each access.
    /// </summary>
    public bool UseSingletonInstances { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticEnumCollectionAttribute"/> class with no parameters.
    /// CollectionName must be set as a named parameter.
    /// </summary>
    public StaticEnumCollectionAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticEnumCollectionAttribute"/> class.
    /// </summary>
    /// <param name="collectionName">The name of the generated static collection class.</param>
    public StaticEnumCollectionAttribute(string collectionName)
    {
        CollectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
    }
}