namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Equality comparison operator (equals).
/// </summary>
public sealed class EqualOperator : ComparisonOperatorBase
{
    public EqualOperator() : base(1, "Equal", "Equal to operator") { }

    /// <inheritdoc/>
    public override string SqlOperator => "=";

    /// <inheritdoc/>
    public override bool IsSingleValue => true;
}