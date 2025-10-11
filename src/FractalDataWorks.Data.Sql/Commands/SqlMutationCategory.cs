using FractalDataWorks.Commands.Abstractions;

namespace FractalDataWorks.Data.Sql.Commands;

/// <summary>
/// Category for SQL mutation commands that modify data.
/// </summary>
public sealed class SqlMutationCategory : CommandCategoryBase
{
    /// <summary>
    /// Gets the singleton instance of the SQL mutation category.
    /// </summary>
    public static SqlMutationCategory Instance { get; } = new();

    private SqlMutationCategory()
        : base(
            id: 2,
            name: "SqlMutation",
            requiresTransaction: true,
            supportsStreaming: false,
            isCacheable: false,
            isMutation: true,
            executionPriority: 100)
    {
    }
}
