using System;
using Microsoft.Extensions.Configuration;

namespace FractalDataWorks.Configuration.Providers.SqlServer;

/// <summary>
/// Configuration source for hierarchical multi-tenant configuration from SQL Server.
/// Implements Microsoft.Extensions.Configuration.IConfigurationSource.
/// </summary>
/// <remarks>
/// This source loads configuration from a SQL Server database table with hierarchical
/// multi-tenant support (DEFAULT → APPLICATION → TENANT → USER).
/// </remarks>
public class SqlServerHierarchicalConfigurationSource : IConfigurationSource
{
    /// <summary>
    /// Gets or sets the service provider for resolving dependencies.
    /// </summary>
    public IServiceProvider ServiceProvider { get; set; } = null!;

    /// <summary>
    /// Gets or sets the section name for this configuration (e.g., "Email", "Database").
    /// </summary>
    public string SectionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant identifier for multi-tenant configuration.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the user identifier for per-user configuration.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the table name containing configuration data.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to enable caching of loaded configuration.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache duration for loaded configuration.
    /// </summary>
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Builds the configuration provider.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <returns>A new instance of the configuration provider.</returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new SqlServerHierarchicalConfigurationProvider(this);
    }
}
