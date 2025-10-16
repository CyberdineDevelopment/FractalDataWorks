using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Abstractions;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Update command for modifying existing records (UPDATE operation).
/// Returns the number of affected rows.
/// </summary>
/// <typeparam name="T">The type of entity to update.</typeparam>
/// <remarks>
/// <para>
/// This command represents a universal UPDATE operation that works across all data sources.
/// Translators convert it to:
/// <list type="bullet">
/// <item>SQL: UPDATE statement with WHERE clause</item>
/// <item>REST: PUT/PATCH request</item>
/// <item>File: Update record</item>
/// <item>GraphQL: updateX mutation</item>
/// </list>
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var customer = new Customer { Id = 123, Name = "Acme Corp Updated", IsActive = false };
/// var command = new UpdateCommand&lt;Customer&gt;("Customers", customer)
/// {
///     Filter = new FilterExpression {
///         Conditions = [
///             new FilterCondition {
///                 PropertyName = "Id",
///                 Operator = FilterOperators.Equal,
///                 Value = 123
///             }
///         ]
///     }
/// };
///
/// var result = await connection.ExecuteAsync(command);
/// // result.Value is int (affected rows) - NO CASTING!
/// </code>
/// </para>
/// </remarks>
[TypeOption(typeof(DataCommands), "Update")]
public sealed class UpdateCommand<T> : DataCommandBase<int, T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCommand{T}"/> class.
    /// </summary>
    /// <param name="containerName">The name of the container to update.</param>
    /// <param name="data">The updated entity data.</param>
    public UpdateCommand(string containerName, T data)
        : base(id: 3, name: "Update", containerName, DataCommandCategory.Update, data)
    {
    }

    /// <summary>
    /// Gets or sets the filter expression (WHERE clause for update).
    /// Determines which records to update.
    /// </summary>
    public IFilterExpression? Filter { get; init; }
}
