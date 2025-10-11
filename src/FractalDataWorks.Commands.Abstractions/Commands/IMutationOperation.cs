namespace FractalDataWorks.Commands.Abstractions.Commands;

/// <summary>
/// Defines mutation operation types.
/// </summary>
public interface IMutationOperation : ICommandCategory
{
    /// <summary>
    /// Gets whether this operation creates new records.
    /// </summary>
    bool CreatesRecords { get; }

    /// <summary>
    /// Gets whether this operation modifies existing records.
    /// </summary>
    bool ModifiesRecords { get; }

    /// <summary>
    /// Gets whether this operation deletes records.
    /// </summary>
    bool DeletesRecords { get; }
}