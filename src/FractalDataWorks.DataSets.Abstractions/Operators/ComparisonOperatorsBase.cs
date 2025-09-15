using FractalDataWorks.Collections;

namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Collection of comparison operators for dataset queries.
/// This partial class will be extended by the source generator to include
/// all discovered comparison operators with high-performance lookup capabilities.
/// </summary>
[TypeCollection(CollectionName = "ComparisonOperators")]
public partial class ComparisonOperatorsBase : TypeCollectionBase<ComparisonOperatorBase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ComparisonOperatorsBase"/> class.
    /// The source generator will populate all discovered comparison operators.
    /// </summary>
    public ComparisonOperatorsBase()
    {
        // Source generator will add:
        // - Static fields for each comparison operator (e.g., Equal, NotEqual, GreaterThan, etc.)
        // - High-performance lookup methods
        // - Empty() method returning default instance
        // - All() method returning all comparison operators
    }
}