using FractalDataWorks.Commands.Abstractions;

namespace FractalDataWorks.Data.Sql.Commands;

/// <summary>
/// Command category for SQL mutation operations.
/// </summary>
/// <remarks>
/// Represents SQL operations that modify data, including
/// INSERT, UPDATE, DELETE, and DDL operations.
/// </remarks>
public sealed class SqlMutationCategory : CommandCategoryBase
{
    /// <summary>
    /// Gets the singleton instance of the SQL mutation category.
    /// </summary>
    public static SqlMutationCategory Instance { get; } = new();

    /// <inheritdoc/>
    public override bool RequiresTransaction => true;

    /// <inheritdoc/>
    public override bool SupportsStreaming => false;

    /// <inheritdoc/>
    public override bool IsCacheable => false;

    /// <inheritdoc/>
    public override bool IsMutation => true;

    /// <inheritdoc/>
    public override int ExecutionPriority => 50;

    private SqlMutationCategory() : base(2, "SqlMutation")
    {
    }
}