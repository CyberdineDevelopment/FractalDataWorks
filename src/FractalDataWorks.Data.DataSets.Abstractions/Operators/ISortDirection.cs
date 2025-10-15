using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.DataSets.Abstractions.Operators;

/// <summary>
/// Interface for sort directions.
/// </summary>
public interface ISortDirection : ITypeOption<SortDirectionBase>
{
    /// <summary>
    /// Gets the description of this sort direction.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the SQL keyword for this sort direction.
    /// </summary>
    string SqlKeyword { get; }

    /// <summary>
    /// Gets a value indicating whether this is an ascending sort direction.
    /// </summary>
    bool IsAscending { get; }
}