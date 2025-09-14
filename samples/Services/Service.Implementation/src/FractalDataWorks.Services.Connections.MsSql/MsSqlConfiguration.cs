using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.MsSql;

/// <summary>
/// Configuration for SQL Server connections.
/// </summary>
public sealed class MsSqlConfiguration : IConnectionConfiguration
{
    /// <inheritdoc />
    public string ConnectionTypeName { get; set; } = "MsSql";

    /// <inheritdoc />
    public string ConnectionString { get; set; } = string.Empty;

    /// <inheritdoc />
    public int CommandTimeout { get; set; } = 30;

    /// <inheritdoc />
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets a value indicating whether to enable connection pooling.
    /// </summary>
    public bool EnableConnectionPooling { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum pool size.
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the minimum pool size.
    /// </summary>
    public int MinPoolSize { get; set; } = 0;

    /// <summary>
    /// Gets or sets the connection lifetime in seconds.
    /// </summary>
    public int ConnectionLifetime { get; set; } = 0;
}