using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.DataStores.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataContainers.Abstractions.Types;

/// <summary>
/// Configuration settings specific to SQL Table data containers.
/// </summary>
public sealed class SqlTableContainerConfiguration : IContainerConfiguration
{
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema name.
    /// </summary>
    public string SchemaName { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets the command timeout in seconds.
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Gets or sets a value indicating whether bulk insert operations are enabled.
    /// </summary>
    public bool EnableBulkInsert { get; set; } = true;

    /// <summary>
    /// Gets or sets the batch size for bulk operations.
    /// </summary>
    public int BatchSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets a value indicating whether to use connection pooling.
    /// </summary>
    public bool UseConnectionPooling { get; set; } = true;

    /// <inheritdoc/>
    public string ContainerType => "SQL Table";

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object> Settings =>
        new Dictionary<string, object>(StringComparer.Ordinal)
        {
            { nameof(TableName), TableName },
            { nameof(SchemaName), SchemaName },
            { nameof(CommandTimeout), CommandTimeout },
            { nameof(EnableBulkInsert), EnableBulkInsert },
            { nameof(BatchSize), BatchSize },
            { nameof(UseConnectionPooling), UseConnectionPooling }
        };

    /// <inheritdoc/>
    public IFdwResult Validate()
    {
        if (string.IsNullOrWhiteSpace(TableName))
            return FdwResult.Failure("Table name is required for SQL Table containers");

        if (CommandTimeout < 0)
            return FdwResult.Failure("Command timeout must be non-negative");

        if (BatchSize <= 0)
            return FdwResult.Failure("Batch size must be greater than zero");

        return FdwResult.Success();
    }

    /// <inheritdoc/>
    public T GetValue<T>(string key, T defaultValue = default!)
    {
        if (Settings.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;
        return defaultValue;
    }
}