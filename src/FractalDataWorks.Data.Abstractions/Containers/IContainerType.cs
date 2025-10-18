using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Represents a container type definition - metadata about data containers.
/// </summary>
/// <remarks>
/// Container types describe data sources like tables, views, API endpoints, files.
/// </remarks>
public interface IContainerType : ITypeOption
{
    /// <summary>
    /// Gets whether this container type supports schema discovery.
    /// </summary>
    bool SupportsSchemaDiscovery { get; }
}
