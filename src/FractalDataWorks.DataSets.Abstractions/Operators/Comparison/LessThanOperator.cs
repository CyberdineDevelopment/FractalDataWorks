namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Less than comparison operator (<).
/// </summary>
public sealed class LessThanOperator : ComparisonOperatorBase
{
    public LessThanOperator() : base(5, "LessThan", "Less than operator") { }

    /// <inheritdoc/>
    public override string SqlOperator => "<";

    /// <inheritdoc/>
    public override bool IsSingleValue => true;
}