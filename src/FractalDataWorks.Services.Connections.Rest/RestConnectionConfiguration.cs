using System.Collections.Generic;
using FluentValidation.Results;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.Rest;

/// <summary>
/// Configuration class for REST connections.
/// Provides REST-specific configuration options.
/// </summary>
public sealed class RestConnectionConfiguration : IConnectionConfiguration
{
    /// <summary>
    /// Gets the connection type name this configuration is for.
    /// </summary>
    public string ConnectionType => "REST";

    /// <summary>
    /// Gets or sets the unique identifier for this configuration instance.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of this configuration for lookup and display.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the section name for this configuration in appsettings.
    /// </summary>
    public string SectionName => "Connections:REST";

    /// <summary>
    /// Gets or sets a value indicating whether this configuration is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the base URL for the REST API.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timeout for HTTP requests in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the default content type for REST requests.
    /// Defaults to "application/json".
    /// </summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// Gets or sets the Accept header value for REST requests.
    /// Defaults to "application/json".
    /// </summary>
    public string AcceptHeader { get; set; } = "application/json";

    /// <summary>
    /// Gets or sets the User-Agent header value for REST requests.
    /// </summary>
    public string UserAgent { get; set; } = "FractalDataWorks-REST-Client/1.0";

    /// <summary>
    /// Gets or sets a value indicating whether to follow HTTP redirects automatically.
    /// Defaults to true.
    /// </summary>
    public bool AllowAutoRedirect { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of automatic redirects to follow.
    /// Defaults to 5.
    /// </summary>
    public int MaxAutomaticRedirections { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether to use compression for requests and responses.
    /// Defaults to true.
    /// </summary>
    public bool UseCompression { get; set; } = true;

    /// <summary>
    /// Gets or sets additional query parameters to include with all requests.
    /// </summary>
    public IReadOnlyDictionary<string, string> DefaultQueryParameters { get; set; } = new Dictionary<string, string>(System.StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets additional HTTP headers to include with all requests.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>(System.StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the retry count for failed requests.
    /// Defaults to 3.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the retry delay in milliseconds between failed requests.
    /// Defaults to 1000ms (1 second).
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 1000;

    /// <summary>
    /// Gets or sets a value indicating whether to validate SSL certificates.
    /// Defaults to true for security.
    /// </summary>
    public bool ValidateSslCertificate { get; set; } = true;

    /// <summary>
    /// Gets or sets the service lifetime for this connection instance.
    /// </summary>
    public IServiceLifetime Lifetime { get; set; } = ServiceLifetimes.Scoped;

    /// <summary>
    /// Validates this configuration using FluentValidation.
    /// </summary>
    /// <returns>A GenericResult containing the FluentValidation ValidationResult.</returns>
    public IGenericResult<ValidationResult> Validate()
    {
        var result = new ValidationResult();

        if (string.IsNullOrEmpty(BaseUrl))
        {
            result.Errors.Add(new ValidationFailure(nameof(BaseUrl), "BaseUrl is required"));
        }

        if (TimeoutSeconds <= 0)
        {
            result.Errors.Add(new ValidationFailure(nameof(TimeoutSeconds), "TimeoutSeconds must be greater than 0"));
        }

        return GenericResult.Success(result);
    }
}