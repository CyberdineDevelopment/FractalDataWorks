using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Authentication.Abstractions.Security;
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Service type definition for JWT Bearer token authentication.
/// Provides stateless authentication using JSON Web Tokens for API scenarios.
/// </summary>
public sealed class JwtBearerAuthenticationType : 
    AuthenticationTypeBase<IAuthenticationService, IAuthenticationConfiguration, IAuthenticationServiceFactory<IAuthenticationService, IAuthenticationConfiguration>>
{
    /// <summary>
    /// Gets the singleton instance of the JWT Bearer authentication type.
    /// </summary>
    public static JwtBearerAuthenticationType Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtBearerAuthenticationType"/> class.
    /// </summary>
    private JwtBearerAuthenticationType() : base(3, "JwtBearer", "Token Authentication")
    {
    }

    /// <inheritdoc/>
    public override string ProviderName => "JWT Bearer Token";

    /// <inheritdoc/>
    public override AuthenticationMethod Method => AuthenticationMethod.Bearer;

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedProtocols => new[]
    {
        "JWT",
        "OAuth 2.0",
        "OpenID Connect"
    };

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedFlows => new[]
    {
        "bearer_token",
        "authorization_header"
    };

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedTokenTypes => new[]
    {
        "JWT",
        "Access Token"
    };

    /// <inheritdoc/>
    public override bool SupportsMultiTenant => true; // JWT can contain tenant information

    /// <inheritdoc/>
    public override bool SupportsTokenCaching => false; // Stateless by design

    /// <inheritdoc/>
    public override bool SupportsTokenRefresh => false; // Bearer tokens are typically short-lived

    /// <inheritdoc/>
    public override int Priority => 70; // High priority for API scenarios

    /// <inheritdoc/>
    public override Type FactoryType => typeof(IAuthenticationServiceFactory<IAuthenticationService, IAuthenticationConfiguration>);

    /// <inheritdoc/>
    public override void Register(IServiceCollection services)
    {
        // Register JWT Bearer specific services
        // services.AddScoped<IJwtBearerAuthenticationFactory, JwtBearerAuthenticationFactory>();
        // services.AddScoped<JwtBearerAuthenticationService>();
        // services.AddScoped<IJwtTokenValidator, JwtTokenValidator>();
        // services.AddScoped<IJwtSecurityTokenHandler, JwtSecurityTokenHandler>();
    }

    /// <inheritdoc/>
    public override void Configure(IConfiguration configuration)
    {
        // JWT Bearer specific configuration
        // This could set validation parameters, signing keys, issuers, etc.
    }
}