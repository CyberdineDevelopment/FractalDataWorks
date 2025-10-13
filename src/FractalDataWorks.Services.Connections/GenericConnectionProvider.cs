using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Messages;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.Connections.Logging;

namespace FractalDataWorks.Services.Connections;

/// <summary>
/// Implementation of IGenericConnectionProvider that uses ConnectionTypes for factory lookup.
/// </summary>
public sealed class GenericConnectionProvider : IGenericConnectionProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GenericConnectionProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericConnectionProvider"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving factories.</param>
    /// <param name="configuration">The configuration for loading connection settings.</param>
    /// <param name="logger">The logger for logging operations.</param>
    public GenericConnectionProvider(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<GenericConnectionProvider> logger)
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
    public async Task<IGenericResult<IGenericConnection>> GetConnection(IConnectionConfiguration configuration)
    {
        if (configuration == null)
        {
            return GenericResult<IGenericConnection>.Failure(new ArgumentNullMessage(nameof(configuration)));
        }

        try
        {
            GenericConnectionProviderLog.GettingConnection(_logger, configuration.ConnectionType);

            // Get the factory from DI using the configuration's ConnectionType
            // This assumes factories are registered by their connection type name
            var factory = _serviceProvider.GetService<IConnectionFactory>();
            if (factory == null)
            {
                GenericConnectionProviderLog.NoFactoryRegistered(_logger, configuration.ConnectionType);
                return GenericResult<IGenericConnection>.Failure(
                    new NotFoundMessage($"No factory registered for connection type: {configuration.ConnectionType}"));
            }

            // Create the connection using the factory
            var result = await factory.CreateConnectionAsync(configuration).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                GenericConnectionProviderLog.ConnectionCreated(_logger, configuration.ConnectionType);
            }
            else
            {
                GenericConnectionProviderLog.ConnectionCreationFailed(_logger, configuration.ConnectionType, result.Error.ToString());
            }

            return result;
        }
        catch (Exception ex)
        {
            GenericConnectionProviderLog.ConnectionCreationException(_logger, ex, configuration.ConnectionType);
            return GenericResult<IGenericConnection>.Failure(new ErrorMessage(ex.Message));
        }
    }

    /// <summary>
    /// Gets a connection by configuration ID.
    /// This would typically load the configuration from a database or configuration store.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration to load.</param>
    /// <returns>A result containing the connection instance or failure information.</returns>
    public Task<IGenericResult<IGenericConnection>> GetConnection(int configurationId)
    {
        // This would typically load configuration from a database
        // For now, return a not implemented error
        GenericConnectionProviderLog.GetConnectionByIdNotImplemented(_logger, configurationId);
        return Task.FromResult<IGenericResult<IGenericConnection>>(GenericResult<IGenericConnection>.Failure(new NotImplementedMessage($"Configuration loading by ID not implemented: {configurationId}")));
    }

    /// <summary>
    /// Gets a connection by configuration name from appsettings.
    /// </summary>
    /// <param name="configurationName">The name of the configuration section.</param>
    /// <returns>A result containing the connection instance or failure information.</returns>
    public async Task<IGenericResult<IGenericConnection>> GetConnection(string configurationName)
    {
        if (string.IsNullOrEmpty(configurationName))
        {
            return GenericResult<IGenericConnection>.Failure(new ArgumentNullMessage("configurationName"));
        }

        try
        {
            GenericConnectionProviderLog.GettingConnectionByConfigurationName(_logger, configurationName);

            // Load configuration section
            var section = _configuration.GetSection($"Connections:{configurationName}");
            if (!section.Exists())
            {
                GenericConnectionProviderLog.ConfigurationSectionNotFound(_logger, configurationName);
                return GenericResult<IGenericConnection>.Failure(new NotFoundMessage($"Configuration section not found: Connections:{configurationName}"));
            }

            // Get the connection type from the section
            var connectionTypeName = section["ConnectionType"];
            if (string.IsNullOrEmpty(connectionTypeName))
            {
                GenericConnectionProviderLog.ConnectionTypeNotSpecified(_logger, configurationName);
                return GenericResult<IGenericConnection>.Failure(new ValidationMessage($"ConnectionType not specified in configuration section: {configurationName}"));
            }

            // Create a minimal configuration placeholder since we don't have concrete binding
            // The factory will need to handle the specific configuration binding based on connection type
            var config = new BasicConnectionConfiguration
            {
                ConnectionType = connectionTypeName,
                Name = configurationName
            };

            // Use the main method to create the connection
            return await GetConnection(config).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            GenericConnectionProviderLog.GetConnectionByNameException(_logger, ex, configurationName);
            return GenericResult<IGenericConnection>.Failure(new ErrorMessage(ex.Message));
        }
    }
}

/// <summary>
/// Basic implementation of IConnectionConfiguration for simple scenarios.
/// </summary>
internal sealed class BasicConnectionConfiguration : IConnectionConfiguration
{
    public string ConnectionType { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public IServiceLifetime Lifetime { get; set; } = ServiceLifetimes.Scoped;

    public static IGenericResult<FluentValidation.Results.ValidationResult> ValidateStatic()
    {
        return GenericResult<FluentValidation.Results.ValidationResult>.Success(new FluentValidation.Results.ValidationResult());
    }

    #pragma warning disable CA1822 // Member does not access instance data - interface requires instance method
    public IGenericResult<FluentValidation.Results.ValidationResult> Validate()
    #pragma warning restore CA1822
    {
        return ValidateStatic();
    }
}