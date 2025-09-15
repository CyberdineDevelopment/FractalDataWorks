namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Inequality comparison operator (!=, <>).
/// </summary>
public sealed class NotEqualOperator : ComparisonOperatorBase
{
    public NotEqualOperator() : base(2, "NotEqual", "Not equal to operator") { }

    /// <inheritdoc/>
    public override string SqlOperator => "!=";

    /// <inheritdoc/>
    public override bool IsSingleValue => true;
}