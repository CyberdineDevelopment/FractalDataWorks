namespace FractalDataWorks.Web.RestEndpoints.Configuration;

/// <summary>
/// API key-specific security configuration.
/// </summary>
public sealed class ApiKeySecurityConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether API key authentication is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the header name for API key authentication.
    /// </summary>
    public string HeaderName { get; init; } = "X-API-Key";

    /// <summary>
    /// Gets or sets the query parameter name for API key authentication.
    /// </summary>
    public string QueryParameterName { get; init; } = "apikey";

    /// <summary>
    /// Gets or sets a value indicating whether API keys can be provided via query parameters.
    /// Should be disabled in production for security.
    /// </summary>
    public bool AllowQueryParameter { get; init; } = false;

    /// <summary>
    /// Gets or sets the valid API keys and their associated metadata.
    /// In production, these should be stored securely.
    /// </summary>
    public Dictionary<string, ApiKeyMetadata> ValidKeys { get; init; } = new(StringComparer.Ordinal);
}