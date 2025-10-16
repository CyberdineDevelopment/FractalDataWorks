using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Data.DataSets.Abstractions.Operators;

/// <summary>
/// Ascending sort direction (smallest to largest, A to Z).
/// </summary>
[TypeOption(typeof(SortDirections), "Ascending")]
public sealed class AscendingDirection : SortDirectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AscendingDirection"/> class.
    /// </summary>
    public AscendingDirection() : base(
        id: 1,
        name: "Ascending",
        description: "Sort from smallest to largest, A to Z",
        isAscending: true,
        sqlKeyword: "ASC")
    {
    }
}