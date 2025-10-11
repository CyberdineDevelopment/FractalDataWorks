using FractalDataWorks.Commands.Abstractions;

namespace FractalDataWorks.Data.Sql.Commands;

/// <summary>
/// Category for SQL bulk operation commands.
/// </summary>
public sealed class SqlBulkCategory : CommandCategoryBase
{
    /// <summary>
    /// Gets the singleton instance of the SQL bulk category.
    /// </summary>
    public static SqlBulkCategory Instance { get; } = new();

    private SqlBulkCategory()
        : base(
            id: 3,
            name: "SqlBulk",
            requiresTransaction: true,
            supportsStreaming: true,
            isCacheable: false,
            isMutation: true,
            executionPriority: 75)
    {
    }
}
