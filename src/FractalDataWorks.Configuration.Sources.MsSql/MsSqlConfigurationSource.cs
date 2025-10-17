using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlKata.Execution;

namespace FractalDataWorks.Configuration.Sources.MsSql;

/// <summary>
/// SQL Server configuration source for hierarchical multi-tenant configuration.
/// Implements Microsoft.Extensions.Configuration.IConfigurationSource.
/// </summary>
/// <remarks>
/// This source loads configuration from a SQL Server database table with hierarchical
/// multi-tenant support (DEFAULT → APPLICATION → TENANT → USER).
/// </remarks>
public class MsSqlConfigurationSource : IConfigurationSource
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
    /// Builds the configuration provider.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <returns>A new instance of the FdwConfigurationProvider.</returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new FdwConfigurationProvider(this, LoadHierarchy, SectionName);
    }

    /// <summary>
    /// Loads hierarchical configuration from SQL Server.
    /// Returns a dictionary keyed by level (0=DEFAULT, 1=APP, 2=TENANT, 3=USER).
    /// </summary>
    public IDictionary<int, IDictionary<string, object>> LoadHierarchy()
    {
        using var scope = ServiceProvider.CreateScope();
        var queryFactory = scope.ServiceProvider.GetRequiredService<QueryFactory>();

        var hierarchy = new Dictionary<int, IDictionary<string, object>>();

        // Load all 4 levels
        for (int level = 0; level < 4; level++)
        {
            var query = BuildLevelQuery(queryFactory, level);
            var result = query.FirstOrDefault<IDictionary<string, object>>();

            if (result != null)
            {
                hierarchy[level] = result;
            }
        }

        return hierarchy;
    }

    /// <summary>
    /// Builds a query for a specific hierarchy level.
    /// </summary>
    private SqlKata.Query BuildLevelQuery(QueryFactory queryFactory, int level)
    {
        var query = queryFactory.Query(TableName)
            .Where("Level", level);

        // TenantId filter
        if (level >= 2 && TenantId != null)
        {
            query.Where("TenantId", TenantId);
        }
        else
        {
            query.WhereNull("TenantId");
        }

        // UserId filter
        if (level == 3 && UserId != null)
        {
            query.Where("UserId", UserId);
        }
        else
        {
            query.WhereNull("UserId");
        }

        return query;
    }
}
