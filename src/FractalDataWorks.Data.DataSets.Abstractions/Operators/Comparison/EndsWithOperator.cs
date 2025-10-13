using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Data.DataSets.Abstractions.Operators;

/// <summary>
/// Ends with comparison operator (LIKE %value).
/// </summary>
[TypeOption(typeof(ComparisonOperators), "EndsWith")]
public sealed class EndsWithOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EndsWithOperator"/> class.
    /// </summary>
    public EndsWithOperator() : base(
        id: 9,
        name: "EndsWith",
        description: "Ends with text operator",
        sqlOperator: "LIKE",
        isSingleValue: true)
    {
    }
}