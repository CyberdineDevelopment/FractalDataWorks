using FractalDataWorks.Commands.Abstractions;
using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Data.Sql.Commands;

/// <summary>
/// SQL command category for query operations.
/// </summary>
public sealed class SqlCommandCategory : CommandCategoryBase, IEnumOption<SqlCommandCategory>
{
    /// <summary>
    /// Gets the query command category.
    /// </summary>
    public static SqlCommandCategory Query { get; } = new(1, "SqlQuery", false, true, true, false);

    /// <summary>
    /// Gets the mutation command category.
    /// </summary>
    public static SqlCommandCategory Mutation { get; } = new(2, "SqlMutation", true, false, false, true);

    /// <summary>
    /// Gets the bulk command category.
    /// </summary>
    public static SqlCommandCategory Bulk { get; } = new(3, "SqlBulk", true, true, false, true);

    /// <inheritdoc/>
    public override bool RequiresTransaction { get; }

    /// <inheritdoc/>
    public override bool SupportsStreaming { get; }

    /// <inheritdoc/>
    public override bool IsCacheable { get; }

    /// <inheritdoc/>
    public override bool IsMutation { get; }

    /// <inheritdoc/>
    public override int ExecutionPriority => IsMutation ? 25 : 75;

    private SqlCommandCategory(int id, string name, bool requiresTransaction,
        bool supportsStreaming, bool isCacheable, bool isMutation)
        : base(id, name)
    {
        RequiresTransaction = requiresTransaction;
        SupportsStreaming = supportsStreaming;
        IsCacheable = isCacheable;
        IsMutation = isMutation;
    }
}