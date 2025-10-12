namespace FractalDataWorks.Data.DataSets.Abstractions.Operators;

/// <summary>
/// Less than comparison operator (&lt;).
/// </summary>
public sealed class LessThanOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LessThanOperator"/> class.
    /// </summary>
    public LessThanOperator() : base(
        id: 5,
        name: "LessThan",
        description: "Less than operator",
        sqlOperator: "<",
        isSingleValue: true)
    {
    }
}