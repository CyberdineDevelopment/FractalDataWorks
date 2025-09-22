namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Equality comparison operator (equals).
/// </summary>
public sealed class EqualOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EqualOperator"/> class.
    /// </summary>
    public EqualOperator() : base(1, "Equal", "Equal to operator") { }

    /// <inheritdoc/>
    public override string SqlOperator => "=";

    /// <inheritdoc/>
    public override bool IsSingleValue => true;
}