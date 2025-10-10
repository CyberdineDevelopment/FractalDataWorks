using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Abstractions;

namespace FractalDataWorks.Data.Sql.Commands;

/// <summary>
/// Command category for SQL query operations.
/// </summary>
/// <remarks>
/// Represents read-only SQL operations that retrieve data
/// without modifying the database state.
/// </remarks>
[TypeOption(typeof(CommandCategories), "SqlQuery")]
public sealed class SqlQueryCategory : CommandCategoryBase
{
    /// <summary>
    /// Gets the singleton instance of the SQL query category.
    /// </summary>
    public static SqlQueryCategory Instance { get; } = new();

    /// <inheritdoc/>
    public override bool RequiresTransaction => false;

    /// <inheritdoc/>
    public override bool SupportsStreaming => true;

    /// <inheritdoc/>
    public override bool IsCacheable => true;

    /// <inheritdoc/>
    public override bool IsMutation => false;

    /// <inheritdoc/>
    public override int ExecutionPriority => 100;

    internal SqlQueryCategory() : base(1, "SqlQuery")
    {
    }
}