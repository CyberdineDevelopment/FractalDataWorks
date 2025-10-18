using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// TypeCollection for all container type implementations.
/// Containers represent data sources like tables, views, API endpoints, files.
/// </summary>
[TypeCollection(typeof(ContainerTypeBase), typeof(IContainerType), typeof(ContainerTypes), RestrictToCurrentCompilation = false)]
public sealed partial class ContainerTypes : TypeCollectionBase<ContainerTypeBase, IContainerType>
{
    // TypeCollectionGenerator will generate all members
}
