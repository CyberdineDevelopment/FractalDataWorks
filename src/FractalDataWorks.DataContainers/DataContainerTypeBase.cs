using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.Collections;
using FractalDataWorks.DataStores.Abstractions;
using FractalDataWorks.DataContainers.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataContainers;

/// <summary>
/// Base class for data container type Enhanced Enums.
/// Inherits from TypeOptionBase to enable automatic collection generation.
/// </summary>
/// <remarks>
/// DataContainerTypeBase provides the foundation for creating strongly-typed container
/// Enhanced Enums that can be discovered and registered automatically by the source generator.
/// Each container type (CSV, JSON, SQL table, etc.) should inherit from this class to
/// participate in the type collection system.
///
/// The source generator will create a static DataContainerTypes class with properties
/// for each container type, along with lookup methods for runtime container discovery.
/// </remarks>
public abstract class DataContainerTypeBase : TypeOptionBase, IDataContainerType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataContainerTypeBase"/> class.
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
        : base(id, name)
    {
        FileExtension = fileExtension;
        MimeType = mimeType;
        SupportsRead = supportsRead;
        SupportsWrite = supportsWrite;
        SupportsSchemaInference = supportsSchemaInference;
        SupportsStreaming = supportsStreaming;
        CompatibleConnectionTypes = compatibleConnectionTypes;
        Category = category;
    }

    /// <inheritdoc/>
    public string? FileExtension { get; }

    /// <inheritdoc/>
    public string? MimeType { get; }

    /// <inheritdoc/>
    public bool SupportsRead { get; }

    /// <inheritdoc/>
    public bool SupportsWrite { get; }

    /// <inheritdoc/>
    public bool SupportsSchemaInference { get; }

    /// <inheritdoc/>
    public bool SupportsStreaming { get; }

    /// <inheritdoc/>
    public IEnumerable<string> CompatibleConnectionTypes { get; }

    /// <summary>
    /// Gets the category for this container type.
    /// </summary>
    public string? Category { get; }

    /// <inheritdoc/>
    public abstract IContainerConfiguration CreateDefaultConfiguration();

    /// <inheritdoc/>
    public abstract IGenericResult<ContainerTypeMetadata> GetTypeMetadata(DataLocation location);


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
