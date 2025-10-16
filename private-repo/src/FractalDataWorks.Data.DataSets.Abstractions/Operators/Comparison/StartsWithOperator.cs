using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Data.DataSets.Abstractions.Operators;

/// <summary>
/// Starts with comparison operator (LIKE value%).
/// </summary>
[TypeOption(typeof(ComparisonOperators), "StartsWith")]
public sealed class StartsWithOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartsWithOperator"/> class.
    /// </summary>
    public StartsWithOperator() : base(
        id: 8,
        name: "StartsWith",
        description: "Starts with text operator",
        sqlOperator: "LIKE",
        isSingleValue: true)
    {
    }
}