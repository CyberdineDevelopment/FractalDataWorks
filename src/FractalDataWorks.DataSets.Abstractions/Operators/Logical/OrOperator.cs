namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Logical OR operator.
/// Combines conditions where either can be true.
/// </summary>
public sealed class OrOperator : LogicalOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrOperator"/> class.
    /// </summary>
    public OrOperator() : base(
        id: 2,
        name: "Or",
        description: "Logical OR - either condition can be true",
        precedence: 1,
        sqlOperator: "OR")
    {
    }
}