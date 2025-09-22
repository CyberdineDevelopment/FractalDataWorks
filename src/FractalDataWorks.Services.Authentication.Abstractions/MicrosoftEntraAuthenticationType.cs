using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Authentication.Abstractions.Security;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Service type definition for Microsoft Entra ID (formerly Azure AD) authentication.
/// Provides OAuth2/OpenID Connect authentication through Microsoft's identity platform.
/// </summary>
public sealed class MicrosoftEntraAuthenticationType : 
    AuthenticationTypeBase<IAuthenticationService, IAuthenticationConfiguration, IAuthenticationServiceFactory<IAuthenticationService, IAuthenticationConfiguration>>
{
    /// <summary>
    /// Gets the singleton instance of the Microsoft Entra authentication type.
    /// </summary>
    public static MicrosoftEntraAuthenticationType Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrosoftEntraAuthenticationType"/> class.
    /// </summary>
    private MicrosoftEntraAuthenticationType() : base(1, "MicrosoftEntra", "Cloud Authentication")
    {
    }

    /// <inheritdoc/>
    public override string ProviderName => "Microsoft Entra ID";

    /// <inheritdoc/>
    public override AuthenticationMethod Method => AuthenticationMethod.OAuth2;

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedProtocols => new[]
    {
        "OAuth 2.0",
        "OpenID Connect",
        "SAML 2.0"
    };

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedFlows => new[]
    {
        "authorization_code",
        "client_credentials",
        "device_code",
        "refresh_token"
    };

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedTokenTypes => new[]
    {
        "JWT",
        "Reference Token"
    };

    /// <inheritdoc/>
    public override bool SupportsMultiTenant => true;

    /// <inheritdoc/>
    public override bool SupportsTokenCaching => true;

    /// <inheritdoc/>
    public override bool SupportsTokenRefresh => true;

    /// <inheritdoc/>
    public override int Priority => 90; // High priority for enterprise scenarios

    /// <inheritdoc/>
    public override Type FactoryType => typeof(IAuthenticationServiceFactory<IAuthenticationService, IAuthenticationConfiguration>);

    /// <inheritdoc/>
    public override void Register(IServiceCollection services)
    {
        // Register Microsoft Entra specific services
        // services.AddScoped<IMicrosoftEntraAuthenticationFactory, MicrosoftEntraAuthenticationFactory>();
        // services.AddScoped<MicrosoftEntraAuthenticationService>();
        // services.AddScoped<EntraTokenCache>();
    }

    /// <inheritdoc/>
    public override void Configure(IConfiguration configuration)
    {
        // Microsoft Entra specific configuration setup
        // This could validate tenant IDs, client IDs, etc.
    }
}