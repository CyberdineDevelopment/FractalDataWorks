using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SqlKata;
using SqlKata.Execution;

namespace FractalDataWorks.Configuration.Providers.SqlServer;

/// <summary>
/// Base repository for loading hierarchical configuration from SQL Server.
/// Supports 4-level hierarchy: DEFAULT (0) → APPLICATION (1) → TENANT (2) → USER (3).
/// </summary>
/// <typeparam name="TEntity">The entity type representing configuration data.</typeparam>
/// <remarks>
/// <para>
/// Hierarchy levels:
/// <list type="bullet">
/// <item><description>Level 0 (DEFAULT): TenantId=NULL, UserId=NULL - System-wide defaults</description></item>
/// <item><description>Level 1 (APPLICATION): TenantId=NULL, UserId=NULL - Application-specific settings</description></item>
/// <item><description>Level 2 (TENANT): TenantId NOT NULL, UserId=NULL - Tenant overrides</description></item>
/// <item><description>Level 3 (USER): TenantId NOT NULL, UserId NOT NULL - User-specific overrides</description></item>
/// </list>
/// </para>
/// <para>
/// Services inherit this class and implement:
/// <list type="bullet">
/// <item><description><see cref="GetTableName"/> - Name of the configuration table</description></item>
/// <item><description><see cref="MapFromDictionary"/> - Map database row to entity</description></item>
/// <item><description>(Optional) <see cref="LoadLevelAsync"/> - Custom loading logic for complex scenarios</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class EmailConfigurationRepository
///     : HierarchicalConfigurationRepositoryBase&lt;EmailConfigurationEntity&gt;
/// {
///     public EmailConfigurationRepository(QueryFactory queryFactory)
///         : base(queryFactory)
///     {
///     }
///
///     protected override string GetTableName() => "EmailConfigurations";
///
///     protected override EmailConfigurationEntity MapFromDictionary(IDictionary&lt;string, object&gt; row)
///     {
///         return new EmailConfigurationEntity
///         {
///             Id = Convert.ToInt64(row["Id"]),
///             Level = Convert.ToInt32(row["Level"]),
///             SmtpHost = row["SmtpHost"]?.ToString() ?? string.Empty,
///             SmtpPort = Convert.ToInt32(row["SmtpPort"])
///         };
///     }
/// }
/// </code>
/// </example>
public abstract class HierarchicalConfigurationRepositoryBase<TEntity>
    where TEntity : class
{
    /// <summary>
    /// SqlKata query factory for database operations.
    /// </summary>
    protected readonly QueryFactory QueryFactory;

    /// <summary>
    /// Initializes a new instance of the repository.
    /// </summary>
    /// <param name="queryFactory">SqlKata query factory with configured database connection.</param>
    protected HierarchicalConfigurationRepositoryBase(QueryFactory queryFactory)
    {
        QueryFactory = queryFactory ?? throw new ArgumentNullException(nameof(queryFactory));
    }

    /// <summary>
    /// Gets configuration for all levels in the hierarchy.
    /// Loads levels in parallel for performance.
    /// </summary>
    /// <param name="tenantId">The tenant identifier (null for default/application levels).</param>
    /// <param name="userId">The user identifier (null for default/application/tenant levels).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A dictionary mapping level (0-3) to configuration entity.</returns>
    public virtual async Task<Dictionary<int, TEntity>> GetHierarchyAsync(
        string? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        // Load all 4 levels in parallel
        var defaultTask = LoadLevelAsync(0, null, null, cancellationToken);
        var appTask = LoadLevelAsync(1, null, null, cancellationToken);
        var tenantTask = tenantId != null
            ? LoadLevelAsync(2, tenantId, null, cancellationToken)
            : Task.FromResult<TEntity?>(null);
        var userTask = tenantId != null && userId != null
            ? LoadLevelAsync(3, tenantId, userId, cancellationToken)
            : Task.FromResult<TEntity?>(null);

        await Task.WhenAll(defaultTask, appTask, tenantTask, userTask);

        // Build hierarchy dictionary
        var hierarchy = new Dictionary<int, TEntity>();

        var defaultConfig = await defaultTask;
        if (defaultConfig != null)
            hierarchy[0] = defaultConfig;

        var appConfig = await appTask;
        if (appConfig != null)
            hierarchy[1] = appConfig;

        var tenantConfig = await tenantTask;
        if (tenantConfig != null)
            hierarchy[2] = tenantConfig;

        var userConfig = await userTask;
        if (userConfig != null)
            hierarchy[3] = userConfig;

        return hierarchy;
    }

    /// <summary>
    /// Loads configuration for a specific hierarchy level.
    /// </summary>
    /// <param name="level">The configuration level (0-3).</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The configuration entity for the level, or null if not found.</returns>
    protected virtual async Task<TEntity?> LoadLevelAsync(
        int level,
        string? tenantId,
        string? userId,
        CancellationToken cancellationToken)
    {
        var query = BuildLevelQuery(level, tenantId, userId);

        var results = await query.GetAsync<IDictionary<string, object>>();
        var row = results.FirstOrDefault();

        return row != null ? MapFromDictionary(row) : null;
    }

    /// <summary>
    /// Builds a SqlKata query for a specific hierarchy level.
    /// </summary>
    /// <param name="level">The configuration level (0-3).</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A SqlKata query configured for the hierarchy level.</returns>
    protected virtual Query BuildLevelQuery(int level, string? tenantId, string? userId)
    {
        var query = QueryFactory.Query(GetTableName())
            .Where("Level", level);

        // TenantId filter based on level
        if (level >= 2) // TENANT or USER level
        {
            query.Where("TenantId", tenantId);
        }
        else // DEFAULT or APPLICATION level
        {
            query.WhereNull("TenantId");
        }

        // UserId filter based on level
        if (level == 3) // USER level
        {
            query.Where("UserId", userId);
        }
        else // DEFAULT, APPLICATION, or TENANT level
        {
            query.WhereNull("UserId");
        }

        return query;
    }

    /// <summary>
    /// Gets the name of the database table containing configuration data.
    /// Must be overridden by derived classes.
    /// </summary>
    /// <returns>The table name (e.g., "EmailConfigurations").</returns>
    protected abstract string GetTableName();

    /// <summary>
    /// Maps a database row (dictionary) to a strongly-typed entity.
    /// Must be overridden by derived classes.
    /// </summary>
    /// <param name="row">The database row as a dictionary of column name to value.</param>
    /// <returns>The mapped entity instance.</returns>
    protected abstract TEntity MapFromDictionary(IDictionary<string, object> row);
}
