using System.Collections.Generic;

namespace FractalDataWorks.Commands.Abstractions.Commands;

/// <summary>
/// Interface for mutation commands that modify data.
/// </summary>
/// <remarks>
/// Mutation commands represent write operations like INSERT, UPDATE, DELETE.
/// They typically require transactions and cannot be cached.
/// </remarks>
public interface IMutationCommand : IDataCommand
{
    /// <summary>
    /// Gets the mutation operation type.
    /// </summary>
    /// <value>The type of mutation (Insert, Update, Delete, Upsert).</value>
    IMutationOperation Operation { get; }

    /// <summary>
    /// Gets the data to be mutated.
    /// </summary>
    /// <value>The data object or collection to insert/update.</value>
    object? Data { get; }

    /// <summary>
    /// Gets the filter criteria for update/delete operations.
    /// </summary>
    /// <value>Criteria to identify records to modify.</value>
    object? FilterCriteria { get; }

    /// <summary>
    /// Gets the fields to update.
    /// </summary>
    /// <value>Map of field names to new values for update operations.</value>
    IReadOnlyDictionary<string, object>? UpdateFields { get; }

    /// <summary>
    /// Gets the conflict resolution strategy.
    /// </summary>
    /// <value>How to handle conflicts during mutation.</value>
    IConflictStrategy? ConflictStrategy { get; }

    /// <summary>
    /// Gets whether to return the affected records.
    /// </summary>
    /// <value>True to return inserted/updated/deleted records.</value>
    bool ReturnAffectedRecords { get; }

    /// <summary>
    /// Gets whether to validate constraints before mutation.
    /// </summary>
    /// <value>True to perform constraint validation.</value>
    bool ValidateConstraints { get; }
}