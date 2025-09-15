namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Starts with comparison operator (LIKE value%).
/// </summary>
public sealed class StartsWithOperator : ComparisonOperatorBase
{
    public StartsWithOperator() : base(8, "StartsWith", "Starts with text operator") { }

    /// <inheritdoc/>
    public override string SqlOperator => "LIKE";

    /// <inheritdoc/>
    public override bool IsSingleValue => true;
}