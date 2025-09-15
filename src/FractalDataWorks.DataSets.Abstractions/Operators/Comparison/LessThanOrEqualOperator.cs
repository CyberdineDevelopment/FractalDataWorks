namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Less than or equal comparison operator (<=).
/// </summary>
public sealed class LessThanOrEqualOperator : ComparisonOperatorBase
{
    public LessThanOrEqualOperator() : base(6, "LessThanOrEqual", "Less than or equal to operator") { }

    /// <inheritdoc/>
    public override string SqlOperator => "<=";

    /// <inheritdoc/>
    public override bool IsSingleValue => true;
}