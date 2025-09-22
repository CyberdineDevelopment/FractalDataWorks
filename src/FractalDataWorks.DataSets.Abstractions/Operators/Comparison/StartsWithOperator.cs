namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Starts with comparison operator (LIKE value%).
/// </summary>
public sealed class StartsWithOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartsWithOperator"/> class.
    /// </summary>
    public StartsWithOperator() : base(8, "StartsWith", "Starts with text operator") { }

    /// <inheritdoc/>
    public override string SqlOperator => "LIKE";

    /// <inheritdoc/>
    public override bool IsSingleValue => true;
}