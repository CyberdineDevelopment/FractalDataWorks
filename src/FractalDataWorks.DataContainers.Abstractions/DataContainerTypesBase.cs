using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.DataContainers.Abstractions;

/// <summary>
/// Collection of data container types for dataset operations.
/// This partial class will be extended by the source generator to include
/// all discovered container types with high-performance lookup capabilities.
/// </summary>
[TypeCollection(CollectionName = "DataContainerTypes")]
public partial class DataContainerTypesBase : TypeCollectionBase<ITypeOption>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataContainerTypesBase"/> class.
    /// The source generator will populate all discovered data container types.
    /// </summary>
    public DataContainerTypesBase()
    {
        // Source generator will add:
        // - Static fields for each container type (e.g., Csv, Json, SqlTable, etc.)
        // - High-performance lookup methods
        // - Empty() method returning default instance
        // - All() method returning all container types
    }
}