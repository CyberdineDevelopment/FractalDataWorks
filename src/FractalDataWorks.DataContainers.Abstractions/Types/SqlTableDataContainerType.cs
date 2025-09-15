using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.DataStores.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataContainers.Abstractions.Types;

/// <summary>
/// SQL Table data container type.
/// Supports structured relational data with strong typing and ACID compliance.
/// </summary>
public sealed class SqlTableDataContainerType : DataContainerTypeBase<SqlTableDataContainerType>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlTableDataContainerType"/> class.
    /// </summary>
    public SqlTableDataContainerType() : base(3, "SQL Table", "Database") { }

    /// <inheritdoc/>
    public override string? FileExtension => null; // Database tables don't have file extensions

    /// <inheritdoc/>
    public override string? MimeType => null; // Database tables don't have MIME types

    /// <inheritdoc/>
    public override bool SupportsRead => true;

    /// <inheritdoc/>
    public override bool SupportsWrite => true;

    /// <inheritdoc/>
    public override bool SupportsSchemaInference => true;

    /// <inheritdoc/>
    public override bool SupportsStreaming => true;

    /// <inheritdoc/>
    public override IEnumerable<string> CompatibleConnectionTypes => new[] { "SqlServer", "PostgreSQL", "MySQL", "SQLite", "Oracle" };

    /// <inheritdoc/>
    public override IContainerConfiguration CreateDefaultConfiguration()
    {
        return new SqlTableContainerConfiguration
        {
            TableName = string.Empty,
            SchemaName = "dbo",
            CommandTimeout = 30,
            EnableBulkInsert = true,
            BatchSize = 1000
        };
    }

    /// <inheritdoc/>
    public override IDataContainer CreateContainer(DataLocation location, IContainerConfiguration configuration)
    {
        if (!(configuration is SqlTableContainerConfiguration sqlConfig))
            throw new ArgumentException("Configuration must be SqlTableContainerConfiguration for SQL Table containers");

        return new SqlTableDataContainer(location, sqlConfig);
    }

    /// <inheritdoc/>
    public override IFdwResult ValidateConfiguration(IContainerConfiguration configuration)
    {
        if (!(configuration is SqlTableContainerConfiguration sqlConfig))
            return FdwResult.Failure("Configuration must be SqlTableContainerConfiguration for SQL Table containers");

        if (string.IsNullOrWhiteSpace(sqlConfig.TableName))
            return FdwResult.Failure("Table name is required for SQL Table containers");

        if (sqlConfig.CommandTimeout < 0)
            return FdwResult.Failure("Command timeout must be non-negative");

        if (sqlConfig.BatchSize <= 0)
            return FdwResult.Failure("Batch size must be greater than zero");

        return FdwResult.Success();
    }

    /// <inheritdoc/>
    protected override IEnumerable<string> GetTypeLimitations()
    {
        return new[]
        {
            "Requires active database connection",
            "Schema changes may require table recreation",
            "Performance depends on database server configuration",
            "Transaction isolation may affect concurrent access",
            "Large result sets may consume significant memory"
        };
    }
}

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
}

/// <summary>
/// SQL Table data container implementation.
/// </summary>
internal sealed class SqlTableDataContainer : IDataContainer
{
    public SqlTableDataContainer(DataLocation location, SqlTableContainerConfiguration configuration)
    {
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public DataLocation Location { get; }
    public IContainerConfiguration Configuration { get; }
    public string ContainerType => "SQL Table";
}