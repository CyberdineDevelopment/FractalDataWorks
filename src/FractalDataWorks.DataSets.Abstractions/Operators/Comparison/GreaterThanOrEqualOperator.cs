namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Greater than or equal comparison operator (>=).
/// </summary>
public sealed class GreaterThanOrEqualOperator : ComparisonOperatorBase
{
    public GreaterThanOrEqualOperator() : base(4, "GreaterThanOrEqual", "Greater than or equal to operator") { }

    /// <inheritdoc/>
    public override string SqlOperator => ">=";

    /// <inheritdoc/>
    public override bool IsSingleValue => true;
}