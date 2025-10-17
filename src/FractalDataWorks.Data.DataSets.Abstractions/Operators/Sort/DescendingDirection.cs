using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Data.DataSets.Abstractions.Operators;

/// <summary>
/// Descending sort direction (largest to smallest, Z to A).
/// </summary>
[TypeOption(typeof(SortDirections), "Descending")]
public sealed class DescendingDirection : SortDirectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DescendingDirection"/> class.
    /// </summary>
    public DescendingDirection() : base(
        id: 2,
        name: "Descending",
        description: "Sort from largest to smallest, Z to A",
        isAscending: false,
        sqlKeyword: "DESC")
    {
    }
}