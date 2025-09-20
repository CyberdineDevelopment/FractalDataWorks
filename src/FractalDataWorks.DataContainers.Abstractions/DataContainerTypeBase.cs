using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.Collections;
using FractalDataWorks.DataStores.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataContainers.Abstractions;

/// <summary>
/// Non-generic base class for data container types to enable collection generation.
/// </summary>
public abstract class DataContainerType : TypeOptionBase<DataContainerType>
{
    protected DataContainerType(
        int id,
        string name,
        string? fileExtension,
        string? mimeType,
        bool supportsRead,
        bool supportsWrite,
        bool supportsSchemaInference,
        bool supportsStreaming,
        IEnumerable<string> compatibleConnectionTypes,
        string? category = null)
        : base(id, name, category ?? "Data Container")
    {
        FileExtension = fileExtension;
        MimeType = mimeType;
        SupportsRead = supportsRead;
        SupportsWrite = supportsWrite;
        SupportsSchemaInference = supportsSchemaInference;
        SupportsStreaming = supportsStreaming;
        CompatibleConnectionTypes = compatibleConnectionTypes;
    }

    /// <summary>
    /// Gets the file extension associated with this container type.
    /// </summary>
    public string? FileExtension { get; }

    /// <summary>
    /// Gets the MIME type associated with this container type.
    /// </summary>
    public string? MimeType { get; }

    /// <summary>
    /// Gets a value indicating whether this container type supports reading.
    /// </summary>
    public bool SupportsRead { get; }

    /// <summary>
    /// Gets a value indicating whether this container type supports writing.
    /// </summary>
    public bool SupportsWrite { get; }

    /// <summary>
    /// Gets a value indicating whether this container type supports schema inference.
    /// </summary>
    public bool SupportsSchemaInference { get; }

    /// <summary>
    /// Gets a value indicating whether this container type supports streaming access.
    /// </summary>
    public bool SupportsStreaming { get; }

    /// <summary>
    /// Gets the connection types that are compatible with this container type.
    /// </summary>
    public IEnumerable<string> CompatibleConnectionTypes { get; }

    /// <summary>
    /// Creates a container configuration instance with default settings.
    /// </summary>
    public abstract IContainerConfiguration CreateDefaultConfiguration();

    /// <summary>
    /// Creates a container instance for the specified location and configuration.
    /// </summary>
    public abstract IDataContainer CreateContainer(DataLocation location, IContainerConfiguration configuration);

    /// <summary>
    /// Validates that a configuration is valid for this container type.
    /// </summary>
    public abstract IFdwResult ValidateConfiguration(IContainerConfiguration configuration);

    /// <summary>
    /// Gets metadata about this container type.
    /// </summary>
    public abstract IFdwResult<ContainerMetadata> GetMetadata(DataLocation location);

    /// <summary>
    /// Discovers the schema from an existing container at the specified location.
    /// </summary>
    public abstract Task<IFdwResult<IDataSchema>> DiscoverSchemaAsync(DataLocation location, int sampleSize = 1000);
}

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
public abstract class DataContainerTypeBase<T> : DataContainerType, IDataContainerType<T> where T : DataContainerTypeBase<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataContainerTypeBase{T}"/> class.
    /// </summary>
    protected DataContainerTypeBase(
        int id,
        string name,
        string? fileExtension,
        string? mimeType,
        bool supportsRead,
        bool supportsWrite,
        bool supportsSchemaInference,
        bool supportsStreaming,
        IEnumerable<string> compatibleConnectionTypes,
        string? category = null)
        : base(id, name, fileExtension, mimeType, supportsRead, supportsWrite,
               supportsSchemaInference, supportsStreaming, compatibleConnectionTypes, category)
    {
    }


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
/// Metadata about a specific data container instance.
/// </summary>
public sealed class ContainerMetadata
{
    public ContainerMetadata(
        string name,
        long? sizeBytes = null,
        DateTime? createdDate = null,
        DateTime? modifiedDate = null,
        IDictionary<string, object>? additionalMetadata = null)
    {
        Name = name;
        SizeBytes = sizeBytes;
        CreatedDate = createdDate;
        ModifiedDate = modifiedDate;
        AdditionalMetadata = (IReadOnlyDictionary<string, object>)(additionalMetadata ?? new Dictionary<string, object>(StringComparer.Ordinal));
    }

    public string Name { get; }
    public long? SizeBytes { get; }
    public DateTime? CreatedDate { get; }
    public DateTime? ModifiedDate { get; }
    public IReadOnlyDictionary<string, object> AdditionalMetadata { get; }
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