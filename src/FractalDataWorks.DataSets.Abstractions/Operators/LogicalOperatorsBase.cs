using FractalDataWorks.Collections;

namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Collection of logical operators for dataset queries.
/// This partial class will be extended by the source generator to include
/// all discovered logical operators with high-performance lookup capabilities.
/// </summary>
[TypeCollection(CollectionName = "LogicalOperators")]
public partial class LogicalOperatorsBase : TypeCollectionBase<LogicalOperatorBase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogicalOperatorsBase"/> class.
    /// The source generator will populate all discovered logical operators.
    /// </summary>
    public LogicalOperatorsBase()
    {
        // Source generator will add:
        // - Static fields for each logical operator (e.g., And, Or, etc.)
        // - High-performance lookup methods
        // - Empty() method returning default instance
        // - All() method returning all logical operators
    }
}