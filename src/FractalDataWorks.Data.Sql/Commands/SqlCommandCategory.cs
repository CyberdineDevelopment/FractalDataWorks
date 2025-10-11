using FractalDataWorks.Commands.Abstractions;

namespace FractalDataWorks.Data.Sql.Commands;

/// <summary>
/// Base category for general SQL commands.
/// </summary>
public sealed class SqlCommandCategory : CommandCategoryBase
{
    /// <summary>
    /// Gets the singleton instance of the SQL command category.
    /// </summary>
    public static SqlCommandCategory Instance { get; } = new();

    private SqlCommandCategory()
        : base(
            id: 4,
            name: "SqlCommand",
            requiresTransaction: false,
            supportsStreaming: false,
            isCacheable: false,
            isMutation: false,
            executionPriority: 50)
    {
    }
}
