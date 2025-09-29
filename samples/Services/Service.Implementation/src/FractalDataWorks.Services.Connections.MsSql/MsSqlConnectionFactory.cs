using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Services.Connections.Abstractions;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Connections.MsSql;

/// <summary>
/// Factory for creating SQL Server connections.
/// </summary>
public sealed class MsSqlConnectionFactory : IConnectionFactory<MsSqlConnection, MsSqlConfiguration>
{
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlConnectionFactory"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    public MsSqlConnectionFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <inheritdoc />
    public string ConnectionTypeName => "MsSql";

    /// <inheritdoc />
    public async Task<IGenericConnection> Create(
        IConnectionConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (configuration is not MsSqlConfiguration msSqlConfig)
        {
            throw new ArgumentException(
                $"Configuration must be of type {nameof(MsSqlConfiguration)}",
                nameof(configuration));
        }

        return await Create(msSqlConfig, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MsSqlConnection> Create(
        MsSqlConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            throw new ArgumentException(
                "Connection string cannot be null or empty.",
                nameof(configuration));
        }

        var logger = _loggerFactory.CreateLogger<MsSqlConnection>();
        var connection = new MsSqlConnection(logger, configuration);

        return Task.FromResult(connection);
    }
}