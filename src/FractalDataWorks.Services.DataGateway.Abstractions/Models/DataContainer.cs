using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Models;

/// <summary>
/// Represents a data container (table, view, collection, etc.) discovered in a data source.
/// </summary>
public sealed class DataContainer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataContainer"/> class.
    /// </summary>
    /// <param name="path">The path to this container.</param>
    /// <param name="name">The name of the container.</param>
    /// <param name="containerType">The type of container.</param>
    /// <param name="schema">Optional schema information.</param>
    /// <param name="metadata">Optional metadata about the container.</param>
    public DataContainer(
        DataPath path,
        string name,
        ContainerType containerType,
        object? schema = null,
        IDictionary<string, object>? metadata = null)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ContainerType = containerType;
        Schema = schema;
        Metadata = metadata ?? new Dictionary<string, object>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Gets the path to this container.
    /// </summary>
    public DataPath Path { get; }

    /// <summary>
    /// Gets the name of the container.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the type of container.
    /// </summary>
    public ContainerType ContainerType { get; }

    /// <summary>
    /// Gets optional schema information.
    /// </summary>
    public object? Schema { get; }

    /// <summary>
    /// Gets metadata about the container.
    /// </summary>
    public IDictionary<string, object> Metadata { get; }
}
