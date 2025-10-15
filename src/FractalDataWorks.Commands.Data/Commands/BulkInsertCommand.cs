using System.Collections.Generic;
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Abstractions;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Bulk insert command for adding multiple records efficiently (BULK INSERT operation).
/// Returns the number of affected rows (inserted records).
/// </summary>
/// <typeparam name="T">The type of entity to insert.</typeparam>
/// <remarks>
/// <para>
/// This command represents a universal BULK INSERT operation optimized for large datasets.
/// Translators convert it to:
/// <list type="bullet">
/// <item>SQL: BULK INSERT, SqlBulkCopy, or batched INSERT statements</item>
/// <item>REST: Batched POST requests</item>
/// <item>File: Batch append records</item>
/// <item>GraphQL: Batched createX mutations</item>
/// </list>
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var customers = new List&lt;Customer&gt;
/// {
///     new Customer { Name = "Acme Corp", IsActive = true },
///     new Customer { Name = "TechCo", IsActive = true },
///     new Customer { Name = "RetailCo", IsActive = false }
/// };
/// var command = new BulkInsertCommand&lt;Customer&gt;("Customers", customers)
/// {
///     BatchSize = 1000  // Optional: Specify batch size for processing
/// };
///
/// var result = await connection.ExecuteAsync(command);
/// // result.Value is int (number of inserted records) - NO CASTING!
/// </code>
/// </para>
/// </remarks>
[TypeOption(typeof(DataCommands), "BulkInsert")]
public sealed class BulkInsertCommand<T> : DataCommandBase<int, IEnumerable<T>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BulkInsertCommand{T}"/> class.
    /// </summary>
    /// <param name="containerName">The name of the container to insert into.</param>
    /// <param name="data">The collection of entities to insert.</param>
    public BulkInsertCommand(string containerName, IEnumerable<T> data)
        : base(id: 6, name: "BulkInsert", containerName, DataCommandCategory.BulkInsert, data)
    {
    }

    /// <summary>
    /// Gets or sets the batch size for processing records.
    /// Determines how many records are inserted per batch operation.
    /// </summary>
    /// <value>
    /// The number of records per batch. Defaults to 0 (translator determines optimal batch size).
    /// </value>
    public int BatchSize { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to continue on error.
    /// If true, failed inserts are skipped and logged. If false, any error stops the operation.
    /// </summary>
    /// <value>True to continue on error; false to stop on first error. Defaults to false.</value>
    public bool ContinueOnError { get; init; }
}
