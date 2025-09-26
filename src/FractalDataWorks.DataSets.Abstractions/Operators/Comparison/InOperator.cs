namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// In comparison operator (IN (value1, value2, ...)).
/// </summary>
public sealed class InOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InOperator"/> class.
    /// </summary>
    public InOperator() : base(
        id: 10,
        name: "In",
        description: "In list of values operator",
        sqlOperator: "IN",
        isSingleValue: false)
    {
    }
}