using System.Collections.Generic;
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Abstractions;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Merge command for insert-or-update operations (MERGE/UPSERT operation).
/// Returns the number of affected rows (inserted or updated).
/// </summary>
/// <typeparam name="T">The type of entity to merge.</typeparam>
/// <remarks>
/// <para>
/// This command represents a universal MERGE (UPSERT) operation that works across all data sources.
/// Translators convert it to:
/// <list type="bullet">
/// <item>SQL: MERGE statement or INSERT ... ON CONFLICT UPDATE</item>
/// <item>REST: POST with conflict resolution or PATCH</item>
/// <item>File: Insert or update record</item>
/// <item>GraphQL: upsertX mutation</item>
/// </list>
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var customer = new Customer { Id = 123, Name = "Acme Corp", IsActive = true };
/// var command = new MergeCommand&lt;Customer&gt;("Customers", customer)
/// {
///     MatchFields = ["Id"]  // Fields to match on
/// };
///
/// var result = await connection.ExecuteAsync(command);
/// // result.Value is int (affected rows) - NO CASTING!
/// </code>
/// </para>
/// </remarks>
[TypeOption(typeof(DataCommands), "Merge")]
public sealed class MergeCommand<T> : DataCommandBase<int, T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MergeCommand{T}"/> class.
    /// </summary>
    /// <param name="containerName">The name of the container to merge into.</param>
    /// <param name="data">The entity data to merge.</param>
    public MergeCommand(string containerName, T data)
        : base(id: 5, name: "Merge", containerName, DataCommandCategory.Merge, data)
    {
    }

    /// <summary>
    /// Gets or sets the fields to match on when determining insert vs update.
    /// Typically primary key fields.
    /// </summary>
    /// <value>A collection of field names to match on. Defaults to empty (use all key fields).</value>
    public IReadOnlyList<string> MatchFields { get; init; } = [];

    /// <summary>
    /// Gets or sets the filter expression for additional merge conditions.
    /// Optional - typically MatchFields is sufficient.
    /// </summary>
    public IFilterExpression? Filter { get; init; }
}
