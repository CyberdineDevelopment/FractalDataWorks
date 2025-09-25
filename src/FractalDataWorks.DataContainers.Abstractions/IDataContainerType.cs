using System.Collections.Generic;
using FractalDataWorks.Collections;
using FractalDataWorks.DataStores.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataContainers.Abstractions;

/// <summary>
/// Interface for data container types that participate in the collection system.
/// Extends ITypeOption to support source generation and type discovery.
/// </summary>
/// <remarks>
/// IDataContainerType defines the contract that all container types must implement
/// to be discovered by the source generator and participate in the unified type system.
/// This interface extends ITypeOption to leverage the existing collection
/// infrastructure for automatic type registration and lookup.
/// </remarks>
public interface IDataContainerType
{
    /// <summary>
    /// Gets the file extension associated with this container type.
    /// </summary>
    /// <value>
    /// The file extension (including the dot) for files of this container type,
    /// or null if not applicable (e.g., for database tables).
    /// </value>
    string? FileExtension { get; }

    /// <summary>
    /// Gets the MIME type associated with this container type.
    /// </summary>
    /// <value>
    /// The MIME type for this container format, or null if not applicable.
    /// </value>
    string? MimeType { get; }

    /// <summary>
    /// Gets a value indicating whether this container type supports reading.
    /// </summary>
    /// <value><c>true</c> if reading is supported; otherwise, <c>false</c>.</value>
    bool SupportsRead { get; }

    /// <summary>
    /// Gets a value indicating whether this container type supports writing.
    /// </summary>
    /// <value><c>true</c> if writing is supported; otherwise, <c>false</c>.</value>
    bool SupportsWrite { get; }

    /// <summary>
    /// Gets a value indicating whether this container type supports schema inference.
    /// </summary>
    /// <value><c>true</c> if schema inference is supported; otherwise, <c>false</c>.</value>
    bool SupportsSchemaInference { get; }

    /// <summary>
    /// Gets a value indicating whether this container type supports streaming access.
    /// </summary>
    /// <value><c>true</c> if streaming is supported; otherwise, <c>false</c>.</value>
    bool SupportsStreaming { get; }

    /// <summary>
    /// Gets the connection types that are compatible with this container type.
    /// </summary>
    /// <value>A collection of connection type names that can work with this container.</value>
    IEnumerable<string> CompatibleConnectionTypes { get; }

    /// <summary>
    /// Creates a container configuration instance with default settings.
    /// </summary>
    /// <returns>A default configuration for this container type.</returns>
    IContainerConfiguration CreateDefaultConfiguration();

    /// <summary>
    /// Creates a container instance for the specified location and configuration.
    /// </summary>
    /// <param name="location">The data location where the container exists or will be created.</param>
    /// <param name="configuration">The configuration settings for the container.</param>
    /// <returns>A container instance configured for this type.</returns>
    IDataContainer CreateContainer(DataLocation location, IContainerConfiguration configuration);

    /// <summary>
    /// Validates that a configuration is valid for this container type.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <returns>A result indicating whether the configuration is valid.</returns>
    IFdwResult ValidateConfiguration(IContainerConfiguration configuration);


    /// <summary>
    /// Gets metadata about this container type's capabilities and limitations.
    /// </summary>
    /// <returns>Metadata describing the container type characteristics.</returns>
    ContainerTypeMetadata GetTypeMetadata();
}