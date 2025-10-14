using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Abstractions;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Insert command for adding new records (INSERT operation).
/// Returns the number of affected rows or identity value.
/// </summary>
/// <typeparam name="T">The type of entity to insert.</typeparam>
/// <remarks>
/// <para>
/// This command represents a universal INSERT operation that works across all data sources.
/// Translators convert it to:
/// <list type="bullet">
/// <item>SQL: INSERT INTO statement</item>
/// <item>REST: POST request</item>
/// <item>File: Append record</item>
/// <item>GraphQL: createX mutation</item>
/// </list>
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var customer = new Customer { Name = "Acme Corp", IsActive = true };
/// var command = new InsertCommand&lt;Customer&gt;("Customers", customer);
///
/// var result = await connection.ExecuteAsync(command);
/// // result.Value is int (identity or affected rows) - NO CASTING!
/// </code>
/// </para>
/// </remarks>
[TypeOption(typeof(DataCommands), "Insert")]
public sealed class InsertCommand<T> : DataCommandBase<int, T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InsertCommand{T}"/> class.
    /// </summary>
    /// <param name="containerName">The name of the container to insert into.</param>
    /// <param name="data">The entity to insert.</param>
    public InsertCommand(string containerName, T data)
        : base(id: 2, name: "Insert", containerName, DataCommandCategory.Insert, data)
    {
    }
}
