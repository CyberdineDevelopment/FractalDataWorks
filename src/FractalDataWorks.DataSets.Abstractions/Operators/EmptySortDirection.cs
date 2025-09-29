using System;

namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Empty placeholder sort direction used by source generators when no sort directions are defined.
/// This type should never be instantiated directly - it exists only for source generation purposes.
/// </summary>
internal sealed class EmptySortDirection : SortDirectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmptySortDirection"/> class.
    /// This constructor should never be called - this type exists only as a placeholder.
    /// </summary>
    public EmptySortDirection() : base(
        id: -1,
        name: "Empty",
        description: "Empty placeholder sort direction",
        isAscending: true,
        sqlKeyword: "",
        category: "System")
    {
        throw new InvalidOperationException("EmptySortDirection should never be instantiated.");
    }
}