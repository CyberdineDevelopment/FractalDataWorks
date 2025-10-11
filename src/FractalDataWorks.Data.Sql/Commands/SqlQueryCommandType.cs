using System.Collections.Generic;
using FractalDataWorks.Commands.Abstractions;

namespace FractalDataWorks.Data.Sql.Commands;

/// <summary>
/// Command type for SQL query operations.
/// </summary>
public sealed class SqlQueryCommandType : CommandTypeBase
{
    /// <summary>
    /// Gets the singleton instance of the SQL query command type.
    /// </summary>
    public static SqlQueryCommandType Instance { get; } = new();

    private SqlQueryCommandType()
        : base(
            id: 1,
            name: "SqlQuery",
            description: "SQL query command for reading data",
            category: SqlQueryCategory.Instance,
            supportedTranslators: new[] { SqlTranslatorType.Instance },
            supportsBatching: true,
            supportsPipelining: true,
            maxBatchSize: 100)
    {
    }
}
