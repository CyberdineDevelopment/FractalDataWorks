namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Ascending sort direction (smallest to largest, A to Z).
/// </summary>
public sealed class AscendingDirection : SortDirectionBase
{
    public AscendingDirection() : base(1, "Ascending", "Sort from smallest to largest, A to Z", true) { }

    /// <inheritdoc/>
    public override string SqlKeyword => "ASC";

}