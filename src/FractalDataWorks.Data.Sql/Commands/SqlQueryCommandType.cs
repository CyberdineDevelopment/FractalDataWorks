using System.Collections.Generic;
using FractalDataWorks.Commands.Abstractions;

namespace FractalDataWorks.Data.Sql.Commands;

/// <summary>
/// Command type for SQL query operations.
/// </summary>
/// <remarks>
/// Represents the metadata and capabilities for SQL query commands
/// that retrieve data from relational databases.
/// </remarks>
public sealed class SqlQueryCommandType : CommandTypeBase
{
    /// <summary>
    /// Gets the singleton instance of the SQL query command type.
    /// </summary>
    public static SqlQueryCommandType Instance { get; } = new();

    /// <inheritdoc/>
    public override ICommandCategory Category => SqlQueryCategory.Instance;

    /// <inheritdoc/>
    public override IReadOnlyCollection<ITranslatorType> SupportedTranslators
    {
        get
        {
            // Will be populated by discovering all translators that handle SQL queries
            return new List<ITranslatorType>();
        }
    }

    /// <inheritdoc/>
    public override bool SupportsBatching => true;

    /// <inheritdoc/>
    public override bool SupportsPipelining => true;

    /// <inheritdoc/>
    public override int MaxBatchSize => 100;

    internal SqlQueryCommandType()
        : base(1, "SqlQuery", "Executes SQL queries to retrieve data from relational databases")
    {
    }
}