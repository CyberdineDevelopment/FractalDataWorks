using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlKata.Execution;

namespace FractalDataWorks.Configuration.Providers.SqlServer;

/// <summary>
/// Configuration provider that loads hierarchical multi-tenant configuration from SQL Server.
/// Implements Microsoft.Extensions.Configuration.ConfigurationProvider.
/// </summary>
public class SqlServerHierarchicalConfigurationProvider : ConfigurationProvider
{
    private readonly SqlServerHierarchicalConfigurationSource _source;

    /// <summary>
    /// Initializes a new instance of the provider.
    /// </summary>
    /// <param name="source">The configuration source.</param>
    public SqlServerHierarchicalConfigurationProvider(SqlServerHierarchicalConfigurationSource source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>
    /// Loads configuration data from SQL Server.
    /// Merges hierarchical configuration: DEFAULT → APPLICATION → TENANT → USER.
    /// </summary>
    public override void Load()
    {
        using var scope = _source.ServiceProvider.CreateScope();
        var queryFactory = scope.ServiceProvider.GetRequiredService<QueryFactory>();

        // Load hierarchy from database
        var hierarchy = LoadHierarchyFromDatabase(queryFactory);

        // Flatten and merge hierarchy into configuration dictionary
        Data = FlattenHierarchy(hierarchy);
    }

    /// <summary>
    /// Loads hierarchical configuration from database.
    /// </summary>
    private Dictionary<int, IDictionary<string, object>> LoadHierarchyFromDatabase(QueryFactory queryFactory)
    {
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
        var query = queryFactory.Query(_source.TableName)
            .Where("Level", level);

        // TenantId filter
        if (level >= 2 && _source.TenantId != null)
        {
            query.Where("TenantId", _source.TenantId);
        }
        else
        {
            query.WhereNull("TenantId");
        }

        // UserId filter
        if (level == 3 && _source.UserId != null)
        {
            query.Where("UserId", _source.UserId);
        }
        else
        {
            query.WhereNull("UserId");
        }

        return query;
    }

    /// <summary>
    /// Flattens hierarchical configuration into a single dictionary.
    /// Higher levels override lower levels (USER > TENANT > APP > DEFAULT).
    /// </summary>
    private Dictionary<string, string> FlattenHierarchy(
        Dictionary<int, IDictionary<string, object>> hierarchy)
    {
        var flattened = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Merge in order: DEFAULT (0) → APPLICATION (1) → TENANT (2) → USER (3)
        // Higher levels override lower levels
        for (int level = 0; level < 4; level++)
        {
            if (hierarchy.TryGetValue(level, out var levelData))
            {
                foreach (var kvp in levelData)
                {
                    // Skip hierarchy metadata columns
                    if (kvp.Key == "Id" || kvp.Key == "Level" ||
                        kvp.Key == "TenantId" || kvp.Key == "UserId" ||
                        kvp.Key == "CreatedAt" || kvp.Key == "ModifiedAt")
                    {
                        continue;
                    }

                    // Add with section prefix
                    var key = string.IsNullOrEmpty(_source.SectionName)
                        ? kvp.Key
                        : $"{_source.SectionName}:{kvp.Key}";

                    flattened[key] = kvp.Value?.ToString() ?? string.Empty;
                }
            }
        }

        return flattened;
    }
}
