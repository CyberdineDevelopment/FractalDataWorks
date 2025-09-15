using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Interface for client authentication methods.
/// </summary>
public interface IClientAuthenticationMethod : IEnumOption<ClientAuthenticationMethod>
{
}

/// <summary>
/// Base class for client authentication methods (OAuth2, SAML, JWT).
/// Used for authenticating end users/clients.
/// </summary>
public abstract class ClientAuthenticationMethod : EnumOptionBase<ClientAuthenticationMethod>, IClientAuthenticationMethod
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClientAuthenticationMethod"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this authentication method.</param>
    /// <param name="name">The name of this authentication method.</param>
    protected ClientAuthenticationMethod(int id, string name) : base(id, name)
    {
    }
}

/// <summary>
/// OAuth2 client authentication method.
/// </summary>
[EnumOption("OAuth2")]
public sealed class OAuth2ClientAuthentication : ClientAuthenticationMethod
{
    public OAuth2ClientAuthentication() : base(1, "OAuth2") { }
}

/// <summary>
/// SAML client authentication method.
/// </summary>
[EnumOption("SAML")]
public sealed class SamlClientAuthentication : ClientAuthenticationMethod
{
    public SamlClientAuthentication() : base(2, "SAML") { }
}

/// <summary>
/// JWT client authentication method.
/// </summary>
[EnumOption("JWT")]
public sealed class JwtClientAuthentication : ClientAuthenticationMethod
{
    public JwtClientAuthentication() : base(3, "JWT") { }
}

/// <summary>
/// Collection of client authentication methods.
/// </summary>
[EnumCollection(CollectionName = "ClientAuthenticationMethods")]
public abstract class ClientAuthenticationMethodCollectionBase : EnumCollectionBase<ClientAuthenticationMethod>
{
}

/// <summary>
/// Interface for connection authentication methods.
/// </summary>
public interface IConnectionAuthenticationMethod : IEnumOption<ConnectionAuthenticationMethod>
{
}

/// <summary>
/// Base class for connection authentication methods (Basic, Certificate, ApiKey).
/// Used for authenticating connections to external services.
/// </summary>
public abstract class ConnectionAuthenticationMethod : EnumOptionBase<ConnectionAuthenticationMethod>, IConnectionAuthenticationMethod
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionAuthenticationMethod"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this authentication method.</param>
    /// <param name="name">The name of this authentication method.</param>
    protected ConnectionAuthenticationMethod(int id, string name) : base(id, name)
    {
    }
}

/// <summary>
/// Basic connection authentication method (username/password).
/// </summary>
[EnumOption("Basic")]
public sealed class BasicConnectionAuthentication : ConnectionAuthenticationMethod
{
    public BasicConnectionAuthentication() : base(1, "Basic") { }
}

/// <summary>
/// Certificate-based connection authentication method.
/// </summary>
[EnumOption("Certificate")]
public sealed class CertificateConnectionAuthentication : ConnectionAuthenticationMethod
{
    public CertificateConnectionAuthentication() : base(2, "Certificate") { }
}

/// <summary>
/// API Key connection authentication method.
/// </summary>
[EnumOption("ApiKey")]
public sealed class ApiKeyConnectionAuthentication : ConnectionAuthenticationMethod
{
    public ApiKeyConnectionAuthentication() : base(3, "ApiKey") { }
}

/// <summary>
/// Collection of connection authentication methods.
/// </summary>
[EnumCollection(CollectionName = "ConnectionAuthenticationMethods")]
public abstract class ConnectionAuthenticationMethodCollectionBase : EnumCollectionBase<ConnectionAuthenticationMethod>
{
}