namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Ends with comparison operator (LIKE %value).
/// </summary>
public sealed class EndsWithOperator : ComparisonOperatorBase
{
    public EndsWithOperator() : base(9, "EndsWith", "Ends with text operator") { }

    /// <inheritdoc/>
    public override string SqlOperator => "LIKE";

    /// <inheritdoc/>
    public override bool IsSingleValue => true;
}