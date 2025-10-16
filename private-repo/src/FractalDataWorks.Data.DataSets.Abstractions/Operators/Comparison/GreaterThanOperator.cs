using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Data.DataSets.Abstractions.Operators;

/// <summary>
/// Greater than comparison operator (>).
/// </summary>
[TypeOption(typeof(ComparisonOperators), "GreaterThan")]
public sealed class GreaterThanOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GreaterThanOperator"/> class.
    /// </summary>
    public GreaterThanOperator() : base(
        id: 3,
        name: "GreaterThan",
        description: "Greater than operator",
        sqlOperator: ">",
        isSingleValue: true)
    {
    }
}