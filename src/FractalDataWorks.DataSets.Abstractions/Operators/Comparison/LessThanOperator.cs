namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Less than comparison operator (<).
/// </summary>
public sealed class LessThanOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LessThanOperator"/> class.
    /// </summary>
    public LessThanOperator() : base(5, "LessThan", "Less than operator") { }

    /// <inheritdoc/>
    public override string SqlOperator => "<";

    /// <inheritdoc/>
    public override bool IsSingleValue => true;
}