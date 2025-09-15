using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using FractalDataWorks.DataSets.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions.Translators;
using FractalDataWorks.Services.Connections.Http.Abstractions;
using FractalDataWorks.Services.Connections.Http.Abstractions.Mappers;
using FractalDataWorks.Services.Connections.Http.Abstractions.Translators;
using FractalDataWorks.Services.Connections.Rest.Logging;
using FractalDataWorks.Services.DataGateway.Abstractions.Commands;

namespace FractalDataWorks.Services.Connections.Rest;

/// <summary>
/// REST external connection service implementation.
/// Provides HTTP-based REST API connectivity with enhanced configuration options.
/// </summary>
public sealed partial class RestService : HttpServiceBase<RestConnectionConfiguration>
{
    private readonly HttpClient _httpClient;
    private readonly RestConnectionConfiguration _configuration;
    private readonly ILogger<RestService> _logger;
    private readonly RestQueryTranslator _queryTranslator;
    private readonly JsonResultMapper _resultMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="loggerFactory">The logger factory for creating connection loggers.</param>
    /// <param name="httpClientFactory">The HTTP client factory for creating managed HTTP clients.</param>
    /// <param name="configuration">The REST connection configuration.</param>
    public RestService(
        ILogger<RestService> logger,
        ILoggerFactory loggerFactory,
        IHttpClientFactory httpClientFactory,
        RestConnectionConfiguration configuration)
        : base(logger, loggerFactory, httpClientFactory, configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _httpClient = CreateNamedHttpClient();
        _queryTranslator = new RestQueryTranslator();
        _resultMapper = new JsonResultMapper();
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
    public override async Task<IFdwResult<TOut>> Execute<TOut>(IConnectionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            RestServiceLog.ExecutingCommand(_logger, command.GetType().Name);

            // For now, return a basic success result
            // In a real implementation, this would handle different command types
            // and make appropriate HTTP requests using the configured HttpClient
            
            var result = await ProcessRestCommand<TOut>(command, cancellationToken).ConfigureAwait(false);
            
            RestServiceLog.CommandSuccess(_logger);
            return result;
        }
        catch (HttpRequestException ex)
        {
            RestServiceLog.HttpRequestFailed(_logger, ex);
            return FdwResult<TOut>.Failure($"REST request failed: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            RestServiceLog.RequestTimeout(_logger, ex);
            return FdwResult<TOut>.Failure("REST request timeout");
        }
        catch (Exception ex)
        {
            RestServiceLog.UnexpectedError(_logger, ex);
            return FdwResult<TOut>.Failure($"REST command execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Processes a REST command and returns the result.
    /// </summary>
    /// <typeparam name="TOut">The expected output type.</typeparam>
    /// <param name="command">The command to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the REST command execution.</returns>
    private async Task<IFdwResult<TOut>> ProcessRestCommand<TOut>(IConnectionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Step 12: RestService now has translator and mapper wired in
            // This is a placeholder implementation that demonstrates the pattern
            // Full query translation will be implemented in later steps
            
            RestServiceLog.UsingTranslator(_logger, _queryTranslator.GetType().Name);
            RestServiceLog.UsingMapper(_logger, _resultMapper.GetType().Name);
            
            // Step 14: Handle DataQueryCommand with full translation flow
            if (command is DataQueryCommand dataQuery)
            {
                return await ProcessDataQueryCommand<TOut>(dataQuery, cancellationToken);
            }
            
            // For non-DataQuery commands, fall back to basic HTTP request
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, BaseUrl ?? "https://api.example.com/data");
            
            var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            
            if (!httpResponse.IsSuccessStatusCode)
            {
                return FdwResult<TOut>.Failure($"HTTP request failed: {httpResponse.StatusCode} {httpResponse.ReasonPhrase}");
            }
            
            // Simulate mapper usage - will be fully implemented when IDataSet infrastructure is ready
            var responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            if (typeof(TOut) == typeof(string))
            {
                return FdwResult<TOut>.Success((TOut)(object)responseContent);
            }
            
            return FdwResult<TOut>.Success(default(TOut)!);
        }
        catch (HttpRequestException ex)
        {
            return FdwResult<TOut>.Failure($"HTTP request failed: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return FdwResult<TOut>.Failure("Request timeout");
        }
        catch (Exception ex)
        {
            return FdwResult<TOut>.Failure($"Command processing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Processes a DataQueryCommand using REST translation and mapping.
    /// </summary>
    /// <typeparam name="TOut">The expected result type.</typeparam>
    /// <param name="dataQuery">The data query command to process.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The query execution result.</returns>
    private async Task<IFdwResult<TOut>> ProcessDataQueryCommand<TOut>(DataQueryCommand dataQuery, CancellationToken cancellationToken)
    {
        try
        {
            RestServiceLog.ProcessingDataQuery(_logger, dataQuery.CommandId, dataQuery.DataSet.Name);
            
            // Step 14: Use translator to convert LINQ expression to REST request
            // Note: This is a simplified implementation - full translator will be enhanced later
            var restRequest = await TranslateDataQuery(dataQuery);
            if (!restRequest.IsSuccess)
            {
                return FdwResult<TOut>.Failure($"Query translation failed: {restRequest.ErrorMessage}");
            }
            
            // Execute the translated REST request
            var httpResponse = await _httpClient.SendAsync(restRequest.Value!, cancellationToken).ConfigureAwait(false);
            
            if (!httpResponse.IsSuccessStatusCode)
            {
                return FdwResult<TOut>.Failure($"REST request failed: {httpResponse.StatusCode} {httpResponse.ReasonPhrase}");
            }
            
            // Step 14: Use mapper to convert REST response to expected result type  
            var mappedResult = await MapRestResponse<TOut>(httpResponse, dataQuery.DataSet);
            if (!mappedResult.IsSuccess)
            {
                return FdwResult<TOut>.Failure($"Result mapping failed: {mappedResult.ErrorMessage}");
            }
            
            RestServiceLog.DataQuerySuccess(_logger, dataQuery.CommandId);
            return mappedResult;
        }
        catch (Exception ex)
        {
            RestServiceLog.DataQueryFailed(_logger, dataQuery.CommandId, ex.Message);
            return FdwResult<TOut>.Failure($"DataQuery processing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Translates a DataQueryCommand into an HTTP request.
    /// </summary>
    /// <param name="dataQuery">The data query to translate.</param>
    /// <returns>The translated HTTP request.</returns>
    private async Task<IFdwResult<HttpRequestMessage>> TranslateDataQuery(DataQueryCommand dataQuery)
    {
        try
        {
            // Step 14: Simplified translation - full implementation will use RestQueryTranslator
            // For now, create a basic GET request to the dataset endpoint
            var endpoint = $"{BaseUrl?.TrimEnd('/')}/{dataQuery.DataSet.Name}";
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            
            // Add basic headers
            request.Headers.Add("Accept", "application/json");
            if (!string.IsNullOrEmpty(UserAgent))
            {
                request.Headers.Add("User-Agent", UserAgent);
            }
            
            // Simulate async operation
            await Task.Delay(1);
            
            return FdwResult<HttpRequestMessage>.Success(request);
        }
        catch (Exception ex)
        {
            return FdwResult<HttpRequestMessage>.Failure($"Translation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Maps an HTTP response to the expected result type.
    /// </summary>
    /// <typeparam name="TOut">The expected result type.</typeparam>
    /// <param name="response">The HTTP response to map.</param>
    /// <param name="dataSet">The dataset schema for mapping guidance.</param>
    /// <returns>The mapped result.</returns>
    private async Task<IFdwResult<TOut>> MapRestResponse<TOut>(HttpResponseMessage response, IDataSet dataSet)
    {
        try
        {
            // Step 14: Simplified mapping - full implementation will use JsonResultMapper
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            // For string types, return the raw content
            if (typeof(TOut) == typeof(string))
            {
                return FdwResult<TOut>.Success((TOut)(object)responseContent);
            }
            
            // For other types, would use JsonResultMapper to deserialize
            // For now, return a placeholder result indicating successful mapping
            return FdwResult<TOut>.Success(default(TOut)!);
        }
        catch (Exception ex)
        {
            return FdwResult<TOut>.Failure($"Mapping failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates and configures the HttpClient instance using the factory.
    /// </summary>
    /// <returns>A configured HttpClient instance.</returns>
    private HttpClient CreateNamedHttpClient()
    {
        // Use a named client specific to this REST service instance
        var clientName = $"RestService_{_configuration.BaseUrl?.GetHashCode(StringComparison.Ordinal) ?? 0}";
        var httpClient = CreateHttpClient(clientName);
        
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
    public static void ConfigureHttpClient(IServiceCollection services, string clientName, RestConnectionConfiguration configuration)
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