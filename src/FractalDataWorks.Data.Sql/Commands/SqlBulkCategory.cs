using FractalDataWorks.Commands.Abstractions;

namespace FractalDataWorks.Data.Sql.Commands;

/// <summary>
/// Command category for SQL bulk operations.
/// </summary>
/// <remarks>
/// Represents high-performance bulk operations for loading
/// large amounts of data, such as BULK INSERT and batch operations.
/// </remarks>
public sealed class SqlBulkCategory : CommandCategoryBase
{
    /// <summary>
    /// Gets the singleton instance of the SQL bulk category.
    /// </summary>
    public static SqlBulkCategory Instance { get; } = new();

    /// <inheritdoc/>
    public override bool RequiresTransaction => true;

    /// <inheritdoc/>
    public override bool SupportsStreaming => true;

    /// <inheritdoc/>
    public override bool IsCacheable => false;

    /// <inheritdoc/>
    public override bool IsMutation => true;

    /// <inheritdoc/>
    public override int ExecutionPriority => 25;

    private SqlBulkCategory() : base(3, "SqlBulk")
    {
    }
}