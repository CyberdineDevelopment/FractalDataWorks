using System;

namespace FractalDataWorks.ServiceTypes.Attributes;

/// <summary>
/// Marks a class as a ServiceType collection for source generation.
/// Applied to classes to enable efficient discovery and generation of high-performance collections.
/// </summary>
/// <param name="baseType">The base service type to collect (e.g., typeof(ConnectionTypeBase)).</param>
/// <param name="defaultReturnType">The default return type for generated methods (e.g., typeof(IConnectionType)).</param>
/// <param name="collectionType">The partial class type being generated (e.g., typeof(ConnectionTypes)).</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ServiceTypeCollectionAttribute(Type baseType, Type defaultReturnType, Type collectionType) : Attribute
{
    /// <summary>
    /// Gets the base service type to collect.
    /// </summary>
    public Type BaseType { get; } = baseType ?? throw new ArgumentNullException(nameof(baseType));

    /// <summary>
    /// Gets the default return type for generated methods.
    /// </summary>
    public Type DefaultReturnType { get; } = defaultReturnType ?? throw new ArgumentNullException(nameof(defaultReturnType));

    /// <summary>
    /// Gets the partial class type being generated.
    /// </summary>
    public Type CollectionType { get; } = collectionType ?? throw new ArgumentNullException(nameof(collectionType));

    /// <summary>
    /// Gets the fully qualified name of the base type (for generator compatibility).
    /// </summary>
    public string BaseTypeName => BaseType.FullName ?? BaseType.Name;

    /// <summary>
    /// Gets the collection name (for generator compatibility).
    /// </summary>
    public string CollectionName => CollectionType.Name;

    /// <summary>
    /// Gets or sets a value indicating whether singleton instances should be used instead of factory methods.
    /// </summary>
    public bool UseSingletonInstances { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate methods for access instead of properties.
    /// When true, generates methods like <c>MsSql()</c> instead of properties like <c>MsSql</c>.
    /// Default is false (generates properties).
    /// </summary>
    public bool UseMethods { get; set; }
}