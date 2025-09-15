using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Connections.Logging;

namespace FractalDataWorks.Services.Connections;

/// <summary>
/// Implementation of IFdwConnectionProvider that uses ConnectionTypes for factory lookup.
/// </summary>
public sealed class FdwConnectionProvider : IFdwConnectionProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FdwConnectionProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FdwConnectionProvider"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving factories.</param>
    /// <param name="configuration">The configuration for loading connection settings.</param>
    /// <param name="logger">The logger for logging operations.</param>
    public FdwConnectionProvider(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<FdwConnectionProvider> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a connection using the provided configuration.
    /// The configuration's ConnectionType property determines which factory to use.
    /// </summary>
    /// <param name="configuration">The configuration containing the connection type and settings.</param>
    /// <returns>A result containing the connection instance or failure information.</returns>
    public async Task<IFdwResult<IFdwConnection>> GetConnection(IConnectionConfiguration configuration)
    {
        if (configuration == null)
        {
            return FdwResult.Failure<IFdwConnection>("Configuration cannot be null");
        }

        try
        {
            FdwConnectionProviderLog.GettingConnection(_logger, configuration.ConnectionType);

            // Look up the connection type by the configuration's ConnectionType property
            // This will use the generated ConnectionTypes static class
            var connectionType = ConnectionTypes.Name(configuration.ConnectionType);
            if (connectionType.IsEmpty)
            {
                FdwConnectionProviderLog.UnknownConnectionType(_logger, configuration.ConnectionType);
                return FdwResult.Failure<IFdwConnection>(
                    $"Unknown connection type: {configuration.ConnectionType}");
            }

            // Get the factory from DI
            var factory = _serviceProvider.GetService(connectionType.FactoryType) as IConnectionFactory;
            if (factory == null)
            {
                FdwConnectionProviderLog.NoFactoryRegistered(_logger, configuration.ConnectionType);
                return FdwResult.Failure<IFdwConnection>(
                    $"No factory registered for connection type: {configuration.ConnectionType}");
            }

            // Create the connection using the factory
            var result = await factory.CreateConnectionAsync(configuration);
            
            if (result.IsSuccess)
            {
                FdwConnectionProviderLog.ConnectionCreated(_logger, configuration.ConnectionType);
            }
            else
            {
                FdwConnectionProviderLog.ConnectionCreationFailed(_logger, configuration.ConnectionType, result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            FdwConnectionProviderLog.ConnectionCreationException(_logger, ex, configuration.ConnectionType);
            return FdwResult.Failure<IFdwConnection>(ex.Message);
        }
    }

    /// <summary>
    /// Gets a connection by configuration ID.
    /// This would typically load the configuration from a database or configuration store.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration to load.</param>
    /// <returns>A result containing the connection instance or failure information.</returns>
    public async Task<IFdwResult<IFdwConnection>> GetConnection(int configurationId)
    {
        // This would typically load configuration from a database
        // For now, return a not implemented error
        FdwConnectionProviderLog.GetConnectionByIdNotImplemented(_logger, configurationId);
        return FdwResult.Failure<IFdwConnection>($"Configuration loading by ID not implemented: {configurationId}");
    }

    /// <summary>
    /// Gets a connection by configuration name from appsettings.
    /// </summary>
    /// <param name="configurationName">The name of the configuration section.</param>
    /// <returns>A result containing the connection instance or failure information.</returns>
    public async Task<IFdwResult<IFdwConnection>> GetConnection(string configurationName)
    {
        if (string.IsNullOrEmpty(configurationName))
        {
            return FdwResult.Failure<IFdwConnection>("Configuration name cannot be null or empty");
        }

        try
        {
            FdwConnectionProviderLog.GettingConnectionByConfigurationName(_logger, configurationName);

            // Load configuration section
            var section = _configuration.GetSection($"Connections:{configurationName}");
            if (!section.Exists())
            {
                FdwConnectionProviderLog.ConfigurationSectionNotFound(_logger, configurationName);
                return FdwResult.Failure<IFdwConnection>($"Configuration section not found: Connections:{configurationName}");
            }

            // Get the connection type from the section
            var connectionTypeName = section["ConnectionType"];
            if (string.IsNullOrEmpty(connectionTypeName))
            {
                FdwConnectionProviderLog.ConnectionTypeNotSpecified(_logger, configurationName);
                return FdwResult.Failure<IFdwConnection>($"ConnectionType not specified in configuration section: {configurationName}");
            }

            // Look up the connection type
            var connectionType = ConnectionTypes.Name(connectionTypeName);
            if (connectionType.IsEmpty)
            {
                FdwConnectionProviderLog.UnknownConnectionTypeInConfiguration(_logger, connectionTypeName);
                return FdwResult.Failure<IFdwConnection>($"Unknown connection type: {connectionTypeName}");
            }

            // Bind to the appropriate configuration type
            var config = section.Get(connectionType.ConfigurationType) as IConnectionConfiguration;
            if (config == null)
            {
                FdwConnectionProviderLog.ConfigurationBindingFailed(_logger, connectionType.ConfigurationType?.Name);
                return FdwResult.Failure<IFdwConnection>($"Failed to bind configuration to {connectionType.ConfigurationType?.Name}");
            }

            // Use the main method to create the connection
            return await GetConnection(config);
        }
        catch (Exception ex)
        {
            FdwConnectionProviderLog.GetConnectionByNameException(_logger, ex, configurationName);
            return FdwResult.Failure<IFdwConnection>(ex.Message);
        }
    }
}