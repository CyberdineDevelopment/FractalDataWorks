namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Not in comparison operator (NOT IN (value1, value2, ...)).
/// </summary>
public sealed class NotInOperator : ComparisonOperatorBase
{
    public NotInOperator() : base(11, "NotIn", "Not in list of values operator") { }

    /// <inheritdoc/>
    public override string SqlOperator => "NOT IN";

    /// <inheritdoc/>
    public override bool IsSingleValue => false;
}