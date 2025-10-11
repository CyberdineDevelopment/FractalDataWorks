using FractalDataWorks.Commands.Abstractions;

namespace FractalDataWorks.Data.Sql.Commands;

/// <summary>
/// Category for SQL query commands that read data without modification.
/// </summary>
public sealed class SqlQueryCategory : CommandCategoryBase
{
    /// <summary>
    /// Gets the singleton instance of the SQL query category.
    /// </summary>
    public static SqlQueryCategory Instance { get; } = new();

    private SqlQueryCategory()
        : base(
            id: 1,
            name: "SqlQuery",
            requiresTransaction: false,
            supportsStreaming: true,
            isCacheable: true,
            isMutation: false,
            executionPriority: 50)
    {
    }
}
