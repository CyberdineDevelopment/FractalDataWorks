namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Contains comparison operator (LIKE %value%).
/// </summary>
public sealed class ContainsOperator : ComparisonOperatorBase
{
    public ContainsOperator() : base(7, "Contains", "Contains text operator") { }

    /// <inheritdoc/>
    public override string SqlOperator => "LIKE";

    /// <inheritdoc/>
    public override bool IsSingleValue => true;
}