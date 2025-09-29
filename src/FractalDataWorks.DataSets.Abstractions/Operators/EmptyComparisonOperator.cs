using System;

namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Empty placeholder comparison operator used by source generators when no comparison operators are defined.
/// This type should never be instantiated directly - it exists only for source generation purposes.
/// </summary>
internal sealed class EmptyComparisonOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyComparisonOperator"/> class.
    /// This constructor should never be called - this type exists only as a placeholder.
    /// </summary>
    public EmptyComparisonOperator() : base(
        id: -1,
        name: "Empty",
        description: "Empty placeholder comparison operator",
        sqlOperator: "",
        isSingleValue: true,
        category: "System")
    {
        throw new InvalidOperationException("EmptyComparisonOperator should never be instantiated.");
    }
}