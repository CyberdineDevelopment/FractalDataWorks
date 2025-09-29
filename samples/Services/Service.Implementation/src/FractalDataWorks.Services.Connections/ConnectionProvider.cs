using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Services.Connections.Abstractions;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Connections;

/// <summary>
/// Default implementation of IConnectionProvider.
/// Manages connection factories and creates connections.
/// </summary>
public sealed partial class ConnectionProvider : IConnectionProvider
{
    private readonly ILogger<ConnectionProvider> _logger;
    private readonly ConcurrentDictionary<string, IConnectionFactory> _factories;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionProvider"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ConnectionProvider(ILogger<ConnectionProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _factories = new ConcurrentDictionary<string, IConnectionFactory>(StringComparer.OrdinalIgnoreCase);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Registered connection factory '{FactoryType}' for connection type '{ConnectionType}'")]
    private partial void LogFactoryRegistered(string factoryType, string connectionType);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Replacing existing factory for connection type '{ConnectionType}'. Old factory: {OldFactory}, New factory: {NewFactory}")]
    private partial void LogFactoryReplaced(string connectionType, string oldFactory, string newFactory);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Creating connection using factory '{FactoryType}' for type '{ConnectionType}'")]
    private partial void LogCreatingConnection(string factoryType, string connectionType);

    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully created connection '{ConnectionId}' of type '{ConnectionType}'")]
    private partial void LogConnectionCreated(string connectionId, string connectionType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create connection of type '{ConnectionType}' using factory '{FactoryType}'")]
    private partial void LogConnectionCreationFailed(Exception ex, string connectionType, string factoryType);

    /// <inheritdoc />
    public IEnumerable<string> GetSupportedConnectionTypes()
    {
        return _factories.Keys.ToList();
    }

    /// <inheritdoc />
    public void RegisterFactory(IConnectionFactory factory)
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        if (string.IsNullOrWhiteSpace(factory.ConnectionTypeName))
        {
            throw new ArgumentException("Connection type name cannot be null or empty.", nameof(factory));
        }

        _factories.AddOrUpdate(
            factory.ConnectionTypeName,
            factory,
            (key, existingFactory) =>
            {
                LogFactoryReplaced(key, existingFactory.GetType().Name, factory.GetType().Name);
                return factory;
            });

        LogFactoryRegistered(factory.GetType().Name, factory.ConnectionTypeName);
    }

    /// <inheritdoc />
    public async Task<IGenericConnection> Create(
        IConnectionConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (string.IsNullOrWhiteSpace(configuration.ConnectionTypeName))
        {
            throw new ArgumentException(
                "Connection type name must be specified in configuration.",
                nameof(configuration));
        }

        if (!_factories.TryGetValue(configuration.ConnectionTypeName, out var factory))
        {
            throw new NotSupportedException(
                $"No factory registered for connection type '{configuration.ConnectionTypeName}'. " +
                $"Supported types: {string.Join(", ", GetSupportedConnectionTypes())}");
        }

        LogCreatingConnection(factory.GetType().Name, configuration.ConnectionTypeName);

        try
        {
            var connection = await factory.Create(configuration, cancellationToken);

            LogConnectionCreated(connection.ConnectionId, configuration.ConnectionTypeName);

            return connection;
        }
        catch (Exception ex)
        {
            LogConnectionCreationFailed(ex, configuration.ConnectionTypeName, factory.GetType().Name);
            throw;
        }
    }

    /// <inheritdoc />
    public bool IsConnectionTypeSupported(string connectionTypeName)
    {
        return !string.IsNullOrWhiteSpace(connectionTypeName) &&
               _factories.ContainsKey(connectionTypeName);
    }
}