using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.Rest;

/// <summary>
/// Factory for creating REST connection instances.
/// </summary>
public sealed class RestConnectionFactory : IConnectionFactory<RestService, RestConnectionConfiguration>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestConnectionFactory"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory for creating HTTP clients.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    public RestConnectionFactory(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <summary>
    /// Creates a REST connection using the provided configuration.
    /// </summary>
    /// <param name="configuration">The REST connection configuration.</param>
    /// <returns>A result containing the created REST service instance.</returns>
    public async Task<IGenericResult<RestService>> CreateAsync(RestConnectionConfiguration configuration)
    {
        try
        {
            // Validate configuration
            var validationResult = configuration.Validate();
            if (!validationResult.IsSuccess)
            {
                return GenericResult.Failure<RestService>($"Configuration validation failed: {validationResult.Error}");
            }

            if (!validationResult.Value.IsValid)
            {
                var errors = string.Join(", ", validationResult.Value.Errors);
                return GenericResult.Failure<RestService>($"Configuration is invalid: {errors}");
            }

            // Create the REST service
            var logger = _loggerFactory.CreateLogger<RestService>();
            var service = new RestService(logger, _httpClientFactory, configuration);

            return await Task.FromResult(GenericResult.Success(service)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return GenericResult.Failure<RestService>($"Failed to create REST connection: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a connection using the base configuration interface.
    /// </summary>
    /// <param name="configuration">The configuration object.</param>
    /// <returns>A result containing the connection instance.</returns>
    public async Task<IGenericResult<IGenericConnection>> CreateConnectionAsync(IGenericConfiguration configuration)
    {
        if (configuration is RestConnectionConfiguration restConfig)
        {
            var result = await CreateAsync(restConfig).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return GenericResult.Success<IGenericConnection>(result.Value);
            }
            return GenericResult.Failure<IGenericConnection>(result.Error);
        }

        return GenericResult.Failure<IGenericConnection>($"Invalid configuration type. Expected {nameof(RestConnectionConfiguration)}, got {configuration?.GetType().Name}");
    }

    /// <summary>
    /// Creates a connection using the base configuration interface with connection type.
    /// </summary>
    /// <param name="configuration">The configuration object.</param>
    /// <param name="connectionType">The connection type name.</param>
    /// <returns>A result containing the connection instance.</returns>
    public async Task<IGenericResult<IGenericConnection>> CreateConnectionAsync(IGenericConfiguration configuration, string connectionType)
    {
        return await CreateConnectionAsync(configuration).ConfigureAwait(false);
    }

    /// <summary>
    /// Validates the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <returns>A result indicating whether the configuration is valid.</returns>
    public IGenericResult ValidateConfiguration(IGenericConfiguration configuration)
    {
        if (configuration is RestConnectionConfiguration restConfig)
        {
            var validationResult = restConfig.Validate();
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            if (!validationResult.Value.IsValid)
            {
                var errors = string.Join(", ", validationResult.Value.Errors);
                return GenericResult.Failure(errors);
            }

            return GenericResult.Success();
        }

        return GenericResult.Failure($"Invalid configuration type. Expected {nameof(RestConnectionConfiguration)}, got {configuration?.GetType().Name}");
    }
}
