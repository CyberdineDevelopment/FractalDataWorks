using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Data.DataSets.Abstractions.Operators;

/// <summary>
/// Logical AND operator.
/// Combines conditions where both must be true.
/// </summary>
[TypeOption(typeof(LogicalOperators), "And")]
public sealed class AndOperator : LogicalOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AndOperator"/> class.
    /// </summary>
    public AndOperator() : base(
        id: 1,
        name: "And",
        description: "Logical AND - both conditions must be true",
        precedence: 2,
        sqlOperator: "AND")
    {
    }
}