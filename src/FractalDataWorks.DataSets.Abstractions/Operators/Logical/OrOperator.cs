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
    public OrOperator() : base(2, "Or", "Logical OR - either condition can be true", 1) { }

    /// <inheritdoc/>
    public override string SqlOperator => "OR";

}