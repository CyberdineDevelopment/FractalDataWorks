namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Descending sort direction (largest to smallest, Z to A).
/// </summary>
public sealed class DescendingDirection : SortDirectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DescendingDirection"/> class.
    /// </summary>
    public DescendingDirection() : base(2, "Descending", "Sort from largest to smallest, Z to A", false) { }

    /// <inheritdoc/>
    public override string SqlKeyword => "DESC";

}