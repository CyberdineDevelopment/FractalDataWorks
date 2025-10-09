using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.MsSql;

/// <summary>
/// Service type definition for Microsoft SQL Server connections.
/// Provides metadata, factory creation, and query translation capabilities for SQL Server connections.
/// </summary>
public sealed class MsSqlConnectionType : ConnectionTypeBase<IGenericConnection, MsSqlConfiguration, IMsSqlConnectionFactory>
{
    /// <summary>
    /// Gets the singleton instance of the SQL Server connection type.
    /// </summary>
    public static MsSqlConnectionType Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlConnectionType"/> class.
    /// </summary>
    private MsSqlConnectionType() : base(2, "MsSql", "Database Connections")
    {
    }

    /// <inheritdoc/>
    public override void Register(IServiceCollection services)
    {
        // Register SQL Server specific services
        services.AddScoped<IMsSqlConnectionFactory, MsSqlConnectionFactory>();
        services.AddScoped<MsSqlService>();
        services.AddScoped<MsSqlCommandTranslator>();
        services.AddScoped<ExpressionTranslator>();
    }

    /// <inheritdoc/>
    public override void Configure(IConfiguration configuration)
    {
        // SQL Server specific configuration setup if needed
        // This could validate connection strings, set up connection pools, etc.
    }
}