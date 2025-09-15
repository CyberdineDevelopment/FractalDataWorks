using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.MsSql;

/// <summary>
/// Service type definition for SQL Server connections.
/// </summary>
public sealed class MsSqlConnectionType : ConnectionTypeBase<MsSqlConnection, MsSqlConfiguration, MsSqlConnectionFactory>
{
    /// <summary>
    /// Gets the singleton instance of the SQL Server connection type.
    /// </summary>
    public static MsSqlConnectionType Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlConnectionType"/> class.
    /// </summary>
    private MsSqlConnectionType() : base(1, "MsSql", "Database Connections")
    {
    }

    /// <inheritdoc/>
    public override Type FactoryType => typeof(MsSqlConnectionFactory);

    /// <inheritdoc/>
    public override void Register(IServiceCollection services)
    {
        // Register SQL Server specific services
        services.AddScoped<MsSqlConnectionFactory>();
        services.AddScoped<MsSqlConnection>();
        // TODO: Add command translator and expression translator when implemented
        // services.AddScoped<MsSqlCommandTranslator>();
        // services.AddScoped<ExpressionTranslator>();
    }

    /// <inheritdoc/>
    public override void Configure(IConfiguration configuration)
    {
        // SQL Server specific configuration setup if needed
        // This could validate connection strings, set up connection pools, etc.
    }
}