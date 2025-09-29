using System.Collections.Generic;
using FractalDataWorks.Configuration;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Connections.Http.Abstractions.EnhancedEnums.HttpProtocols;

namespace FractalDataWorks.Services.Connections.Http.Abstractions;

/// <summary>
/// Base configuration class for HTTP external connections.
/// Concrete implementations should inherit from this class and provide specific HTTP configuration.
/// </summary>
public abstract class HttpConnectionConfigurationBase<TConfiguration> : ConfigurationBase<TConfiguration>, IConnectionConfiguration
    where TConfiguration : HttpConnectionConfigurationBase<TConfiguration>, new()
{
    /// <inheritdoc/>
    public override string SectionName => "FdwConnections:Http";

    /// <summary>
    /// Gets or sets the base URL for the HTTP connection.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP protocol type (REST, SOAP, GraphQL).
    /// Concrete implementations should provide a specific protocol instance.
    /// </summary>
    public IHttpProtocol? Protocol { get; set; }

    /// <summary>
    /// Gets or sets the timeout for HTTP requests in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the authentication type.
    /// </summary>
    public string AuthenticationType { get; set; } = "None";

    /// <summary>
    /// Gets or sets additional headers to include with requests.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>(System.StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the content type for requests.
    /// </summary>
    public string ContentType { get; set; } = "application/json";

    /// <inheritdoc/>
    public abstract string ConnectionType { get; set; }

    /// <inheritdoc/>
    public virtual IServiceLifetime LifetimeBase { get; set; } = ServiceLifetimes.Scoped;
}