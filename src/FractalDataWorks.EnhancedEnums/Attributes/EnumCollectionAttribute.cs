using System;

namespace FractalDataWorks.EnhancedEnums.Attributes;

/// <summary>
/// Marks a class as an Enhanced Enum collection for source generation.
/// Applied to collection classes to enable efficient discovery and generation of high-performance collections.
/// Follows the same pattern as TypeCollectionAttribute and ServiceTypeCollectionAttribute.
/// </summary>
/// <param name="baseType">The base enum type to collect (e.g., typeof(ExecutionStatus)).</param>
/// <param name="defaultReturnType">The default return type for generated methods (e.g., typeof(IExecutionStatus)).</param>
/// <param name="collectionType">The partial class type being generated (e.g., typeof(ExecutionStatuses)).</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class EnumCollectionAttribute(Type baseType, Type defaultReturnType, Type collectionType) : Attribute
{
    /// <summary>
    /// Gets the base enum type to collect.
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
}