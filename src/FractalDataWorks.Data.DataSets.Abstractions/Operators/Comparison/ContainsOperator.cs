namespace FractalDataWorks.Data.DataSets.Abstractions.Operators;

/// <summary>
/// Contains comparison operator (LIKE %value%).
/// </summary>
public sealed class ContainsOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContainsOperator"/> class.
    /// </summary>
    public ContainsOperator() : base(
        id: 7,
        name: "Contains",
        description: "Contains text operator",
        sqlOperator: "LIKE",
        isSingleValue: true)
    {
    }
}