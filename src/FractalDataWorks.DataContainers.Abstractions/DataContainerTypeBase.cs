using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Collections;
using FractalDataWorks.DataStores.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataContainers.Abstractions;

/// <summary>
/// Base class for data container type definitions that can be discovered by source generation.
/// Inherits from TypeOptionBase to enable automatic collection generation.
/// </summary>
/// <remarks>
/// DataContainerTypeBase provides the foundation for creating strongly-typed container
/// definitions that can be discovered and registered automatically by the source generator.
/// Each container type (CSV, JSON, SQL table, etc.) should inherit from this class to
/// participate in the type collection system.
/// 
/// The source generator will create a static DataContainerTypes class with properties
/// for each container type, along with lookup methods for runtime container discovery.
/// </remarks>
public abstract class DataContainerTypeBase<T> : TypeOptionBase<T>, IDataContainerType<T> where T : DataContainerTypeBase<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataContainerTypeBase{T}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this container type.</param>
    /// <param name="name">The name of the container type.</param>
    /// <param name="category">The category or grouping for this container type.</param>
    protected DataContainerTypeBase(int id, string name, string? category = null) 
        : base(id, name, category ?? "Data Container")
    {
    }

    /// <summary>
    /// Gets the file extension associated with this container type.
    /// </summary>
    /// <value>
    /// The file extension (including the dot) for files of this container type,
    /// or null if not applicable (e.g., for database tables).
    /// Examples: ".csv", ".json", ".parquet", null for SQL tables.
    /// </value>
    public abstract string? FileExtension { get; }

    /// <summary>
    /// Gets the MIME type associated with this container type.
    /// </summary>
    /// <value>
    /// The MIME type for this container format, or null if not applicable.
    /// Examples: "text/csv", "application/json", "application/octet-stream".
    /// </value>
    public abstract string? MimeType { get; }

    /// <summary>
    /// Gets a value indicating whether this container type supports reading.
    /// </summary>
    /// <value><c>true</c> if reading is supported; otherwise, <c>false</c>.</value>
    public abstract bool SupportsRead { get; }

    /// <summary>
    /// Gets a value indicating whether this container type supports writing.
    /// </summary>
    /// <value><c>true</c> if writing is supported; otherwise, <c>false</c>.</value>
    public abstract bool SupportsWrite { get; }

    /// <summary>
    /// Gets a value indicating whether this container type supports schema inference.
    /// </summary>
    /// <value><c>true</c> if schema inference is supported; otherwise, <c>false</c>.</value>
    public abstract bool SupportsSchemaInference { get; }

    /// <summary>
    /// Gets a value indicating whether this container type supports streaming access.
    /// </summary>
    /// <value><c>true</c> if streaming is supported; otherwise, <c>false</c>.</value>
    public abstract bool SupportsStreaming { get; }

    /// <summary>
    /// Gets the connection types that are compatible with this container type.
    /// </summary>
    /// <value>
    /// A collection of connection type names that can work with this container.
    /// For example, CSV containers might support "File" and "Http" connections.
    /// </value>
    public abstract IEnumerable<string> CompatibleConnectionTypes { get; }

    /// <summary>
    /// Creates a container configuration instance with default settings.
    /// </summary>
    /// <returns>A default configuration for this container type.</returns>
    /// <remarks>
    /// This method creates a configuration instance with sensible defaults
    /// for the container type. The configuration can be customized after creation.
    /// </remarks>
    public abstract IContainerConfiguration CreateDefaultConfiguration();

    /// <summary>
    /// Creates a container instance for the specified location and configuration.
    /// </summary>
    /// <param name="location">The data location where the container exists or will be created.</param>
    /// <param name="configuration">The configuration settings for the container.</param>
    /// <returns>A container instance configured for this type.</returns>
    /// <remarks>
    /// This factory method creates concrete container instances that implement
    /// the IDataContainer interface. The container will be configured according
    /// to the provided location and configuration settings.
    /// </remarks>
    public abstract IDataContainer CreateContainer(DataLocation location, IContainerConfiguration configuration);

    /// <summary>
    /// Validates that a configuration is valid for this container type.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <returns>A result indicating whether the configuration is valid.</returns>
    /// <remarks>
    /// This method performs container-type-specific validation of configuration
    /// settings. It should check for required settings, valid value ranges,
    /// and logical consistency of the configuration.
    /// </remarks>
    public abstract IFdwResult ValidateConfiguration(IContainerConfiguration configuration);


    /// <summary>
    /// Gets metadata about this container type's capabilities and limitations.
    /// </summary>
    /// <returns>
    /// Metadata describing performance characteristics, limitations, and features.
    /// </returns>
    public virtual ContainerTypeMetadata GetTypeMetadata()
    {
        return new ContainerTypeMetadata(
            containerType: Name,
            supportsRead: SupportsRead,
            supportsWrite: SupportsWrite,
            supportsStreaming: SupportsStreaming,
            supportsSchemaInference: SupportsSchemaInference,
            fileExtension: FileExtension,
            mimeType: MimeType,
            compatibleConnections: CompatibleConnectionTypes.ToList(),
            limitations: GetTypeLimitations());
    }

    /// <summary>
    /// Gets a list of limitations or constraints for this container type.
    /// </summary>
    /// <returns>
    /// A collection of human-readable descriptions of type limitations.
    /// </returns>
    /// <remarks>
    /// The default implementation returns an empty list. Override this method
    /// to provide specific limitations such as "Maximum file size 2GB" or
    /// "Does not support nested objects".
    /// </remarks>
    protected virtual IEnumerable<string> GetTypeLimitations()
    {
        return Enumerable.Empty<string>();
    }
}

/// <summary>
/// Metadata about a data container type's capabilities and characteristics.
/// </summary>
/// <remarks>
/// ContainerTypeMetadata provides comprehensive information about what a container
/// type can and cannot do, which is useful for automatic container selection,
/// validation, and performance optimization.
/// </remarks>
public sealed class ContainerTypeMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerTypeMetadata"/> class.
    /// </summary>
    /// <param name="containerType">The container type name.</param>
    /// <param name="supportsRead">Whether reading is supported.</param>
    /// <param name="supportsWrite">Whether writing is supported.</param>
    /// <param name="supportsStreaming">Whether streaming is supported.</param>
    /// <param name="supportsSchemaInference">Whether schema inference is supported.</param>
    /// <param name="fileExtension">The associated file extension.</param>
    /// <param name="mimeType">The associated MIME type.</param>
    /// <param name="compatibleConnections">Compatible connection types.</param>
    /// <param name="limitations">Known limitations.</param>
    /// <param name="additionalMetadata">Additional metadata.</param>
    public ContainerTypeMetadata(
        string containerType,
        bool supportsRead,
        bool supportsWrite,
        bool supportsStreaming,
        bool supportsSchemaInference,
        string? fileExtension = null,
        string? mimeType = null,
        IEnumerable<string>? compatibleConnections = null,
        IEnumerable<string>? limitations = null,
        IDictionary<string, object>? additionalMetadata = null)
    {
        ContainerType = containerType ?? throw new ArgumentNullException(nameof(containerType));
        SupportsRead = supportsRead;
        SupportsWrite = supportsWrite;
        SupportsStreaming = supportsStreaming;
        SupportsSchemaInference = supportsSchemaInference;
        FileExtension = fileExtension;
        MimeType = mimeType;
        CompatibleConnections = compatibleConnections?.ToList() ?? new List<string>();
        Limitations = limitations?.ToList() ?? new List<string>();
        AdditionalMetadata = additionalMetadata != null
            ? new Dictionary<string, object>(additionalMetadata, StringComparer.Ordinal)
            : new Dictionary<string, object>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Gets the container type name.
    /// </summary>
    /// <value>The name of the container type.</value>
    public string ContainerType { get; }

    /// <summary>
    /// Gets a value indicating whether this container type supports reading.
    /// </summary>
    /// <value><c>true</c> if reading is supported; otherwise, <c>false</c>.</value>
    public bool SupportsRead { get; }

    /// <summary>
    /// Gets a value indicating whether this container type supports writing.
    /// </summary>
    /// <value><c>true</c> if writing is supported; otherwise, <c>false</c>.</value>
    public bool SupportsWrite { get; }

    /// <summary>
    /// Gets a value indicating whether this container type supports streaming.
    /// </summary>
    /// <value><c>true</c> if streaming is supported; otherwise, <c>false</c>.</value>
    public bool SupportsStreaming { get; }

    /// <summary>
    /// Gets a value indicating whether this container type supports schema inference.
    /// </summary>
    /// <value><c>true</c> if schema inference is supported; otherwise, <c>false</c>.</value>
    public bool SupportsSchemaInference { get; }

    /// <summary>
    /// Gets the file extension associated with this container type.
    /// </summary>
    /// <value>The file extension, or null if not applicable.</value>
    public string? FileExtension { get; }

    /// <summary>
    /// Gets the MIME type associated with this container type.
    /// </summary>
    /// <value>The MIME type, or null if not applicable.</value>
    public string? MimeType { get; }

    /// <summary>
    /// Gets the connection types compatible with this container.
    /// </summary>
    /// <value>A list of compatible connection type names.</value>
    public IReadOnlyList<string> CompatibleConnections { get; }

    /// <summary>
    /// Gets known limitations of this container type.
    /// </summary>
    /// <value>A list of limitation descriptions.</value>
    public IReadOnlyList<string> Limitations { get; }

    /// <summary>
    /// Gets additional metadata about this container type.
    /// </summary>
    /// <value>Additional properties and information.</value>
    public IReadOnlyDictionary<string, object> AdditionalMetadata { get; }
}