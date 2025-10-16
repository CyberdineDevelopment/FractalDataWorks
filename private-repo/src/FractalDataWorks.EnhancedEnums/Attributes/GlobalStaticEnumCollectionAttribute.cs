using System;

namespace FractalDataWorks.EnhancedEnums.Attributes;

/// <summary>
/// Marks a class as a global static enum collection that discovers enum options across all referenced assemblies
/// and always inherits base class methods.
/// The source generator will create a static class with all discovered enum options from all assemblies
/// and automatically reconstruct all methods from the base collection class.
/// </summary>
/// <remarks>
/// This attribute enables cross-assembly discovery of enum options.
/// Unlike GlobalEnumCollection, this always inherits base methods without requiring InheritsBaseMethods.
/// Use this when you want to aggregate enum options from multiple projects/assemblies.
/// For local discovery only, use StaticEnumCollectionAttribute instead.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class GlobalStaticEnumCollectionAttribute : Attribute
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
    /// Initializes a new instance of the <see cref="GlobalStaticEnumCollectionAttribute"/> class with no parameters.
    /// CollectionName must be set as a named parameter.
    /// </summary>
    public GlobalStaticEnumCollectionAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalStaticEnumCollectionAttribute"/> class.
    /// </summary>
    /// <param name="collectionName">The name of the generated static collection class.</param>
    public GlobalStaticEnumCollectionAttribute(string collectionName)
    {
        CollectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
    }
}