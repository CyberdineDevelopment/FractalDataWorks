using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public virtual IFdwResult<ContainerTypeMetadata> GetTypeMetadata()
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
