using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Data.Abstractions;

namespace FractalDataWorks.Services.Data;

/// <summary>
/// Implementation of data provider that uses DataTypes for service lookup.
/// Follows the ServiceType pattern documented in Services.Abstractions README.
/// </summary>
public sealed class DataProvider : IDataProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DataProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataProvider"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving factories.</param>
    /// <param name="configuration">The configuration for loading data settings.</param>
    /// <param name="logger">The logger for logging operations.</param>
    public DataProvider(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<DataProvider> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a data service using the provided configuration.
    /// The configuration's DataType property determines which factory to use.
    /// </summary>
    /// <param name="configuration">The configuration containing the data type and settings.</param>
    /// <returns>A result containing the data service instance or failure information.</returns>
    public async Task<IFdwResult<IDataService>> GetDataService(IDataConfiguration configuration)
    {
        if (configuration == null)
        {
            return FdwResult.Failure<IDataService>("Configuration cannot be null");
        }

        try
        {
            _logger.LogDebug("Getting data service for type: {DataType}", configuration.DataType);

            // Look up the data type by the configuration's DataType property
            // This will use the generated DataTypes static class
            var dataType = DataTypes.Name(configuration.DataType);
            if (dataType.IsEmpty)
            {
                _logger.LogWarning("Unknown data type: {DataType}", configuration.DataType);
                return FdwResult.Failure<IDataService>(
                    $"Unknown data type: {configuration.DataType}");
            }

            // Get the factory from DI
            var factory = _serviceProvider.GetService(dataType.FactoryType) as IDataServiceFactory;
            if (factory == null)
            {
                _logger.LogWarning("No factory registered for data type: {DataType}", configuration.DataType);
                return FdwResult.Failure<IDataService>(
                    $"No factory registered for data type: {configuration.DataType}");
            }

            // Create the data service using the factory
            var result = await factory.CreateServiceAsync(configuration);

            if (result.IsSuccess)
            {
                _logger.LogDebug("Data service created successfully for type: {DataType}", configuration.DataType);
            }
            else
            {
                _logger.LogWarning("Data service creation failed for type: {DataType}, Error: {Error}",
                    configuration.DataType, result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception getting data service for type: {DataType}", configuration.DataType);
            return FdwResult.Failure<IDataService>(ex.Message);
        }
    }
}