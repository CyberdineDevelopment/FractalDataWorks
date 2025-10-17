using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace FractalDataWorks.Configuration.Providers.SqlServer;

/// <summary>
/// Extension methods for registering hierarchical configuration services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SqlKata QueryFactory to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSqlKataQueryFactory(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped(sp =>
        {
            var connection = new SqlConnection(connectionString);
            var compiler = new SqlServerCompiler();
            return new QueryFactory(connection, compiler);
        });

        return services;
    }

    /// <summary>
    /// Adds SqlKata QueryFactory with configuration action.
    /// </summary>
    public static IServiceCollection AddSqlKataQueryFactory(
        this IServiceCollection services,
        Action<SqlKataOptions> configureOptions)
    {
        var options = new SqlKataOptions();
        configureOptions(options);

        return AddSqlKataQueryFactory(services, options.ConnectionString);
    }

    /// <summary>
    /// Adds hierarchical configuration for a specific settings type.
    /// </summary>
    /// <typeparam name="TSettings">The settings POCO type.</typeparam>
    /// <typeparam name="TEntity">The database entity type.</typeparam>
    /// <typeparam name="TRepository">The repository type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional configuration options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHierarchicalConfiguration<TSettings, TEntity, TRepository>(
        this IServiceCollection services,
        Action<HierarchicalConfigurationOptions>? configureOptions = null)
        where TSettings : class
        where TEntity : class
        where TRepository : HierarchicalConfigurationRepositoryBase<TEntity>
    {
        var options = new HierarchicalConfigurationOptions();
        configureOptions?.Invoke(options);

        // Register repository
        services.AddScoped<TRepository>();

        // Configure IOptions with hierarchical provider
        services.AddOptions<TSettings>()
            .Configure<TRepository>((settings, repository) =>
            {
                // This will be called per-request with scoped repository
                // Implementation depends on your ITenantContext
            });

        return services;
    }
}

/// <summary>
/// Options for SqlKata configuration.
/// </summary>
public class SqlKataOptions
{
    /// <summary>
    /// Gets or sets the SQL Server connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}

/// <summary>
/// Options for hierarchical configuration.
/// </summary>
public class HierarchicalConfigurationOptions
{
    /// <summary>
    /// Gets or sets the section name for this configuration.
    /// </summary>
    public string SectionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database table name.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to enable caching.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache duration.
    /// </summary>
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(5);
}
