using System;

namespace FractalDataWorks.ServiceTypes.Attributes;

/// <summary>
/// Marks a class as a ServiceType collection for source generation.
/// Applied to classes that inherit from ServiceTypeCollectionBase to enable efficient discovery.
/// </summary>
/// <param name="baseTypeName">The fully qualified name of the base service type (e.g., "ConnectionTypeBase").</param>
/// <param name="collectionName">Optional override for the generated collection name. If null, removes "Base" suffix from class name.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ServiceTypeCollectionAttribute(string baseTypeName, string? collectionName = null) : Attribute
{
    /// <summary>
    /// Gets the fully qualified name of the base service type.
    /// </summary>
    public string BaseTypeName { get; } = baseTypeName ?? throw new ArgumentNullException(nameof(baseTypeName));

    /// <summary>
    /// Gets the optional override for the generated collection name.
    /// </summary>
    public string? CollectionName { get; } = collectionName;
}