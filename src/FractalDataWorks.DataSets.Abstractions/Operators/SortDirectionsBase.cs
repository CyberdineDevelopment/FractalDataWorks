using FractalDataWorks.Collections;

namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Collection of sort directions for dataset queries.
/// This partial class will be extended by the source generator to include
/// all discovered sort directions with high-performance lookup capabilities.
/// </summary>
[TypeCollection(CollectionName = "SortDirections")]
public partial class SortDirectionsBase : TypeCollectionBase<SortDirectionBase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SortDirectionsBase"/> class.
    /// The source generator will populate all discovered sort directions.
    /// </summary>
    public SortDirectionsBase()
    {
        // Source generator will add:
        // - Static fields for each sort direction (e.g., Ascending, Descending)
        // - High-performance lookup methods
        // - Empty() method returning default instance
        // - All() method returning all sort directions
    }
}