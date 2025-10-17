using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace FractalDataWorks.Configuration.Sources.MsSql;

/// <summary>
/// Extension methods for registering SqlKata query factory.
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
