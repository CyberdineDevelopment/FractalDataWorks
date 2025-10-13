using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Data.DataSets.Abstractions.Operators;

/// <summary>
/// Equality comparison operator (equals).
/// </summary>
[TypeOption(typeof(ComparisonOperators), "Equal")]
public sealed class EqualOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EqualOperator"/> class.
    /// </summary>
    public EqualOperator() : base(
        id: 1,
        name: "Equal",
        description: "Equal to operator",
        sqlOperator: "=",
        isSingleValue: true)
    {
    }
}