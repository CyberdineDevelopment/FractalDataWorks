using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Connections.Rest.Logging;

namespace FractalDataWorks.Services.Connections.Rest;

/// <summary>
/// REST external connection service implementation.
/// Provides HTTP-based REST API connectivity with enhanced configuration options.
/// </summary>
public sealed partial class RestService : ConnectionServiceBase<IConnectionCommand, RestConnectionConfiguration, RestService>
{
    private readonly HttpClient _httpClient;
    private readonly RestConnectionConfiguration _configuration;
    private readonly ILogger<RestService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="httpClientFactory">The HTTP client factory for creating managed HTTP clients.</param>
    /// <param name="configuration">The REST connection configuration.</param>
    public RestService(
        ILogger<RestService> logger,
        IHttpClientFactory httpClientFactory,
        RestConnectionConfiguration configuration)
        : base(logger, configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        if (httpClientFactory == null)
        {
            throw new ArgumentNullException(nameof(httpClientFactory));
        }

        _httpClient = CreateHttpClient(httpClientFactory);
    }

    /// <inheritdoc/>
    public override string ServiceType => "REST";

    /// <summary>
    /// Gets the base URL for this REST service.
    /// </summary>
    public string? BaseUrl => _configuration.BaseUrl;

    /// <summary>
    /// Gets the user agent string for this REST service.
    /// </summary>
    public string UserAgent => _configuration.UserAgent;

    /// <summary>
    /// Gets the HttpClient instance for making requests.
    /// </summary>
    public HttpClient HttpClient => _httpClient;

    /// <inheritdoc/>
    public override async Task<IGenericResult<TOut>> Execute<TOut>(IConnectionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            RestServiceLog.ExecutingCommand(_logger, command.GetType().Name);

            var result = await ProcessRestCommand<TOut>(command, cancellationToken).ConfigureAwait(false);

            RestServiceLog.CommandSuccess(_logger);
            return result;
        }
        catch (HttpRequestException ex)
        {
            RestServiceLog.HttpRequestFailed(_logger, ex);
            return GenericResult<TOut>.Failure($"REST request failed: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            RestServiceLog.RequestTimeout(_logger, ex);
            return GenericResult<TOut>.Failure("REST request timeout");
        }
        catch (Exception ex)
        {
            RestServiceLog.UnexpectedError(_logger, ex);
            return GenericResult<TOut>.Failure($"REST command execution failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public override async Task<IGenericResult> Execute(IConnectionCommand command, CancellationToken cancellationToken)
    {
        var result = await Execute<object>(command, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? GenericResult.Success() : GenericResult.Failure(result.CurrentMessage);
    }

    /// <inheritdoc/>
    public override async Task<IGenericResult<TOut>> Execute<TOut>(IConnectionCommand command)
    {
        return await Execute<TOut>(command, CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    /// Processes a REST command and returns the result.
    /// </summary>
    /// <typeparam name="TOut">The expected output type.</typeparam>
    /// <param name="command">The command to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the REST command execution.</returns>
    private async Task<IGenericResult<TOut>> ProcessRestCommand<TOut>(IConnectionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Basic HTTP request implementation
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, BaseUrl ?? "https://api.example.com/data");

            var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return GenericResult<TOut>.Failure($"HTTP request failed: {httpResponse.StatusCode} {httpResponse.ReasonPhrase}");
            }

            var responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (typeof(TOut) == typeof(string))
            {
                return GenericResult<TOut>.Success((TOut)(object)responseContent);
            }

            return GenericResult<TOut>.Success(default(TOut)!);
        }
        catch (HttpRequestException ex)
        {
            return GenericResult<TOut>.Failure($"HTTP request failed: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return GenericResult<TOut>.Failure("Request timeout");
        }
        catch (Exception ex)
        {
            return GenericResult<TOut>.Failure($"Command processing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates and configures the HttpClient instance using the factory.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <returns>A configured HttpClient instance.</returns>
    private HttpClient CreateHttpClient(IHttpClientFactory httpClientFactory)
    {
        var httpClient = httpClientFactory.CreateClient("RestService");

        // Configure the client
        if (!string.IsNullOrEmpty(_configuration.BaseUrl))
        {
            httpClient.BaseAddress = new Uri(_configuration.BaseUrl);
        }

        httpClient.Timeout = TimeSpan.FromSeconds(_configuration.TimeoutSeconds);

        // Set default headers
        httpClient.DefaultRequestHeaders.Clear();

        if (!string.IsNullOrEmpty(_configuration.AcceptHeader))
        {
            httpClient.DefaultRequestHeaders.Add("Accept", _configuration.AcceptHeader);
        }

        if (!string.IsNullOrEmpty(_configuration.UserAgent))
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", _configuration.UserAgent);
        }

        // Add custom headers from configuration
        foreach (var header in _configuration.Headers)
        {
            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        return httpClient;
    }

    /// <summary>
    /// Configures the HTTP client handler for advanced scenarios.
    /// This should be called during service registration to configure the named client.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="clientName">The name of the HTTP client.</param>
    /// <param name="configuration">The REST configuration.</param>
    public static void ConfigureHttpClient(Microsoft.Extensions.DependencyInjection.IServiceCollection services, string clientName, RestConnectionConfiguration configuration)
    {
        services.AddHttpClient(clientName)
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();

                if (!configuration.ValidateSslCertificate)
                {
#pragma warning disable MA0039 // Do not write your own certificate validation method - this is intentional for testing scenarios
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#pragma warning restore MA0039
                }

                handler.AllowAutoRedirect = configuration.AllowAutoRedirect;
                handler.MaxAutomaticRedirections = configuration.MaxAutomaticRedirections;

                return handler;
            });
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient?.Dispose();
        }

        base.Dispose(disposing);
    }
}
