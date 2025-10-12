using System;

namespace FractalDataWorks.Data.DataSets.Abstractions.Operators;

/// <summary>
/// Empty placeholder logical operator used by source generators when no logical operators are defined.
/// This type should never be instantiated directly - it exists only for source generation purposes.
/// </summary>
internal sealed class EmptyLogicalOperator : LogicalOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyLogicalOperator"/> class.
    /// This constructor should never be called - this type exists only as a placeholder.
    /// </summary>
    public EmptyLogicalOperator() : base(
        id: -1,
        name: "Empty",
        description: "Empty placeholder logical operator",
        precedence: 0,
        sqlOperator: "",
        category: "System")
    {
        throw new InvalidOperationException("EmptyLogicalOperator should never be instantiated.");
    }
}