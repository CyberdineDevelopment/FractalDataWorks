namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Greater than comparison operator (>).
/// </summary>
public sealed class GreaterThanOperator : ComparisonOperatorBase
{
    public GreaterThanOperator() : base(3, "GreaterThan", "Greater than operator") { }

    /// <inheritdoc/>
    public override string SqlOperator => ">";

    /// <inheritdoc/>
    public override bool IsSingleValue => true;
}