namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Logical AND operator.
/// Combines conditions where both must be true.
/// </summary>
public sealed class AndOperator : LogicalOperatorBase
{
    public AndOperator() : base(1, "And", "Logical AND - both conditions must be true", 2) { }

    /// <inheritdoc/>
    public override string SqlOperator => "AND";

}