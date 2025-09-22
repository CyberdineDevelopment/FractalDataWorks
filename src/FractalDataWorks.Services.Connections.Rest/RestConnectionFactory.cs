using System;
using System.Net.Http;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Configuration.Abstractions;

namespace FractalDataWorks.Services.Connections.Rest;

/// <summary>
/// Factory for creating REST connection instances.
/// </summary>
public sealed class RestConnectionFactory : IConnectionFactory<RestService, RestConnectionConfiguration>
{
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestConnectionFactory"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory for creating HTTP clients.</param>
    public RestConnectionFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    /// <summary>
    /// Creates a REST connection using the provided configuration.
    /// </summary>
    /// <param name="configuration">The REST connection configuration.</param>
    /// <returns>A result containing the created REST service instance.</returns>
    public async Task<IFdwResult<RestService>> CreateAsync(RestConnectionConfiguration configuration)
    {
        try
        {
            // Validate configuration
            var validationResult = configuration.Validate();
            if (!validationResult.IsSuccess)
            {
                return FdwResult.Failure<RestService>($"Configuration validation failed: {validationResult.Error}");
            }

            if (!validationResult.Value.IsValid)
            {
                var errors = string.Join(", ", validationResult.Value.Errors);
                return FdwResult.Failure<RestService>($"Configuration is invalid: {errors}");
            }

            // Create HTTP client
            var httpClient = _httpClientFactory.CreateClient("RestService");
            httpClient.BaseAddress = new Uri(configuration.BaseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(configuration.TimeoutSeconds);

            // Configure headers
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Accept", configuration.AcceptHeader);
            httpClient.DefaultRequestHeaders.Add("User-Agent", configuration.UserAgent);

            // Create the REST service
            var service = new RestService(httpClient, configuration);
            
            return FdwResult.Success(service);
        }
        catch (Exception ex)
        {
            return FdwResult.Failure<RestService>($"Failed to create REST connection: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a connection using the base configuration interface.
    /// </summary>
    /// <param name="configuration">The configuration object.</param>
    /// <returns>A result containing the connection instance.</returns>
    public async Task<IFdwResult<IFdwConnection>> CreateConnectionAsync(IFdwConfiguration configuration)
    {
        if (configuration is RestConnectionConfiguration restConfig)
        {
            var result = await CreateAsync(restConfig);
            if (result.IsSuccess)
            {
                return FdwResult.Success<IFdwConnection>(result.Value);
            }
            return FdwResult.Failure<IFdwConnection>(result.Error);
        }

        return FdwResult.Failure<IFdwConnection>($"Invalid configuration type. Expected {nameof(RestConnectionConfiguration)}, got {configuration?.GetType().Name}");
    }

    /// <summary>
    /// Creates a connection using the base configuration interface with connection type.
    /// </summary>
    /// <param name="configuration">The configuration object.</param>
    /// <param name="connectionType">The connection type name.</param>
    /// <returns>A result containing the connection instance.</returns>
    public async Task<IFdwResult<IFdwConnection>> CreateConnectionAsync(IFdwConfiguration configuration, string connectionType)
    {
        return await CreateConnectionAsync(configuration);
    }

    /// <summary>
    /// Validates the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <returns>A result indicating whether the configuration is valid.</returns>
    public IFdwResult ValidateConfiguration(IFdwConfiguration configuration)
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
                return FdwResult.Failure(errors);
            }

            return FdwResult.Success();
        }

        return FdwResult.Failure($"Invalid configuration type. Expected {nameof(RestConnectionConfiguration)}, got {configuration?.GetType().Name}");
    }
}