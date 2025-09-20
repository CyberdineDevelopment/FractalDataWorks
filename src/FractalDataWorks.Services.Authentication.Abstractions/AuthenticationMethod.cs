namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Represents the method of authentication used by an authentication service.
/// </summary>
public enum AuthenticationMethod
{
    /// <summary>
    /// Username and password authentication.
    /// </summary>
    UsernamePassword,

    /// <summary>
    /// JWT Bearer token authentication.
    /// </summary>
    JwtBearer,

    /// <summary>
    /// OAuth2 authentication.
    /// </summary>
    OAuth2,

    /// <summary>
    /// SAML authentication.
    /// </summary>
    Saml,

    /// <summary>
    /// API key authentication.
    /// </summary>
    ApiKey,

    /// <summary>
    /// Certificate-based authentication.
    /// </summary>
    Certificate,

    /// <summary>
    /// Windows/Kerberos authentication.
    /// </summary>
    Windows,

    /// <summary>
    /// Multi-factor authentication.
    /// </summary>
    MultiFactor
}