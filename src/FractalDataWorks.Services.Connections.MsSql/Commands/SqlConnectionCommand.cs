using System;
using System.Collections.Generic;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.MsSql.Commands;

/// <summary>
/// Command for executing SQL queries that have been translated from LINQ expressions.
/// </summary>
public sealed class SqlConnectionCommand : IConnectionCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlConnectionCommand"/> class.
    /// </summary>
    /// <param name="sqlText">The SQL text to execute.</param>
    /// <param name="parameters">The SQL parameters.</param>
    public SqlConnectionCommand(string sqlText, IReadOnlyDictionary<string, object>? parameters = null)
    {
        SqlText = sqlText ?? throw new ArgumentNullException(nameof(sqlText));
        Parameters = parameters ?? new Dictionary<string, object>();
        CommandId = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the SQL text to execute.
    /// </summary>
    public string SqlText { get; }

    /// <summary>
    /// Gets the SQL parameters.
    /// </summary>
    public IReadOnlyDictionary<string, object> Parameters { get; }

    /// <inheritdoc/>
    public Guid CommandId { get; }

    /// <inheritdoc/>
    public DateTime CreatedAt { get; }

    /// <inheritdoc/>
    public string CommandType => "SqlConnection";

    /// <inheritdoc/>
    public string ConnectionName => "Default";

    /// <inheritdoc/>
    public string ProviderType => "MsSql";

    /// <inheritdoc/>
    public IGenericResult Validate()
    {
        if (string.IsNullOrWhiteSpace(SqlText))
        {
            return GenericResult.Failure("SQL text cannot be null or empty");
        }

        return GenericResult.Success();
    }
}