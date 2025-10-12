using System;
using System.Collections.Generic;
using FractalDataWorks.Web.Http.Abstractions.Security;
using ISecurityMethod = FractalDataWorks.Web.Http.Abstractions.Security.ISecurityMethod;

namespace FractalDataWorks.Web.RestEndpoints.Configuration;

/// <summary>
/// Security configuration for the FractalDataWorks Web Framework.
/// Provides centralized security settings and validation.
/// </summary>
public sealed class SecurityConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether security is enabled globally.
    /// When disabled, all security checks are bypassed (development only).
    /// </summary>
    public bool SecurityEnabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the default security method to apply to all endpoints.
    /// Individual endpoints can override this setting.
    /// </summary>
    public string DefaultSecurityMethod { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the default security method by name.
    /// Following TypeCollection pattern: retrieve object via Name(string).
    /// </summary>
    // TODO: Enable once SecurityMethods source generator creates Name(string) method
    public ISecurityMethod? DefaultSecurityMethodEnum => null; // SecurityMethods.Name(DefaultSecurityMethod);

    /// <summary>
    /// Gets or sets the JWT security configuration.
    /// </summary>
    public JwtSecurityConfiguration Jwt { get; init; } = new();

    /// <summary>
    /// Gets or sets the API key security configuration.
    /// </summary>
    public ApiKeySecurityConfiguration ApiKey { get; init; } = new();

    /// <summary>
    /// Gets or sets the OAuth2 security configuration.
    /// </summary>
    public OAuth2SecurityConfiguration OAuth2 { get; init; } = new();

    /// <summary>
    /// Gets or sets the certificate security configuration.
    /// </summary>
    public CertificateSecurityConfiguration Certificate { get; init; } = new();

    /// <summary>
    /// Gets or sets the CORS security configuration.
    /// </summary>
    public CorsSecurityConfiguration Cors { get; init; } = new();

    /// <summary>
    /// Gets or sets additional security headers to include in responses.
    /// </summary>
    public Dictionary<string, string> SecurityHeaders { get; init; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["X-Content-Type-Options"] = "nosniff",
        ["X-Frame-Options"] = "DENY",
        ["X-XSS-Protection"] = "1; mode=block",
        ["Referrer-Policy"] = "strict-origin-when-cross-origin"
    };

    /// <summary>
    /// Gets or sets a value indicating whether detailed security error messages should be returned.
    /// Should be disabled in production environments.
    /// </summary>
    public bool DetailedSecurityErrors { get; init; } = false;

    /// <summary>
    /// Validates the security configuration.
    /// </summary>
    /// <returns>True if the configuration is valid; otherwise, false.</returns>
    public bool IsValid()
    {
        // TODO: Implement IsValid
        return true;
    }
}