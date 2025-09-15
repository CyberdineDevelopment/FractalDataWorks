namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// In comparison operator (IN (value1, value2, ...)).
/// </summary>
public sealed class InOperator : ComparisonOperatorBase
{
    public InOperator() : base(10, "In", "In list of values operator") { }

    /// <inheritdoc/>
    public override string SqlOperator => "IN";

    /// <inheritdoc/>
    public override bool IsSingleValue => false;
}