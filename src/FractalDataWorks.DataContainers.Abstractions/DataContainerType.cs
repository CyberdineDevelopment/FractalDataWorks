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
    /// <summary>
    /// Initializes a new instance of the <see cref="DataContainerType"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this container type.</param>
    /// <param name="name">The name of the container type.</param>
    /// <param name="fileExtension">The file extension associated with this container type.</param>
    /// <param name="mimeType">The MIME type for this container type.</param>
    /// <param name="supportsRead">Whether this container type supports read operations.</param>
    /// <param name="supportsWrite">Whether this container type supports write operations.</param>
    /// <param name="supportsSchemaInference">Whether this container type supports schema inference.</param>
    /// <param name="supportsStreaming">Whether this container type supports streaming operations.</param>
    /// <param name="compatibleConnectionTypes">The connection types compatible with this container.</param>
    /// <param name="category">The category for organizing container types.</param>
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