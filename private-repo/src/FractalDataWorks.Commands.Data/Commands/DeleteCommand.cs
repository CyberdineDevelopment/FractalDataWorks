using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Abstractions;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Delete command for removing records (DELETE operation).
/// Returns the number of affected rows.
/// </summary>
/// <remarks>
/// <para>
/// This command represents a universal DELETE operation that works across all data sources.
/// Translators convert it to:
/// <list type="bullet">
/// <item>SQL: DELETE statement with WHERE clause</item>
/// <item>REST: DELETE request</item>
/// <item>File: Remove records</item>
/// <item>GraphQL: deleteX mutation</item>
/// </list>
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var command = new DeleteCommand("Customers")
/// {
///     Filter = new FilterExpression {
///         Conditions = [
///             new FilterCondition {
///                 PropertyName = "IsActive",
///                 Operator = FilterOperators.Equal,
///                 Value = false
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
[TypeOption(typeof(DataCommands), "Delete")]
public sealed class DeleteCommand : DataCommandBase<int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteCommand"/> class.
    /// </summary>
    /// <param name="containerName">The name of the container to delete from.</param>
    public DeleteCommand(string containerName)
        : base(id: 4, name: "Delete", containerName, DataCommandCategory.Delete)
    {
    }

    /// <summary>
    /// Gets or sets the filter expression (WHERE clause for delete).
    /// Determines which records to delete.
    /// </summary>
    public IFilterExpression? Filter { get; init; }
}
