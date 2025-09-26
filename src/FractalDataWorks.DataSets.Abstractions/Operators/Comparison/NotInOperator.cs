namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Not in comparison operator (NOT IN (value1, value2, ...)).
/// </summary>
public sealed class NotInOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotInOperator"/> class.
    /// </summary>
    public NotInOperator() : base(
        id: 11,
        name: "NotIn",
        description: "Not in list of values operator",
        sqlOperator: "NOT IN",
        isSingleValue: false)
    {
    }
}