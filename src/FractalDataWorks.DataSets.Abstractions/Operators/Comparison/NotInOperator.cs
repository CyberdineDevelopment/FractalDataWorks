namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Not in comparison operator (NOT IN (value1, value2, ...)).
/// </summary>
public sealed class NotInOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotInOperator"/> class.
    /// </summary>
    public NotInOperator() : base(11, "NotIn", "Not in list of values operator") { }

    /// <inheritdoc/>
    public override string SqlOperator => "NOT IN";

    /// <inheritdoc/>
    public override bool IsSingleValue => false;
}