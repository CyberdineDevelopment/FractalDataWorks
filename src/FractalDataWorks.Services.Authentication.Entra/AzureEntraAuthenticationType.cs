using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.Authentication;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;
using FractalDataWorks.Services.Authentication.Abstractions.Security;

namespace FractalDataWorks.Services.Authentication.AzureEntra;

/// <summary>
/// Service type definition for Azure Entra (Azure Active Directory) authentication.
/// Inherits from AuthenticationTypeBase for discovery by ServiceTypeCollectionGenerator.
/// </summary>
/// <remarks>
/// This class provides metadata about the Azure Entra authentication provider
/// capabilities and requirements. It will be discovered by the generator and included
/// in the static AuthenticationTypes collection as AuthenticationTypes.AzureEntra.
/// </remarks>
[ServiceTypeOption(typeof(AuthenticationTypes), "AzureEntra")]
public sealed class AzureEntraAuthenticationType : 
    AuthenticationTypeBase<IAuthenticationService, IEntraAuthenticationServiceFactory, IAuthenticationConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureEntraAuthenticationType"/> class.
    /// </summary>
    public AzureEntraAuthenticationType()
        : base(
            id: 1,
            name: "AzureEntra",
            providerName: "Microsoft Entra ID",
            method: new OAuth2AuthenticationMethod(),
            supportedProtocols: [
                new OAuth2Protocol(),
                new OpenIDConnectProtocol(),
                new SAML2Protocol()
            ],
            supportedFlows: [
                new AuthorizationCodeFlow(),
                new ClientCredentialsFlow(),
                new InteractiveFlow()
            ],
            supportedTokenTypes: [
                new AccessToken(),
                new IdToken(),
                new RefreshToken()
            ],
            supportsMultiTenant: true,
            supportsTokenCaching: true,
            supportsTokenRefresh: true,
            category: "Authentication")
    {
    }

    /// <inheritdoc/>
    public override int Priority => 90;

    /// <inheritdoc/>
    public override void Configure(IConfiguration configuration)
    {
        // No additional configuration needed at the type level
        // Configuration is handled by the service itself
    }

    /// <inheritdoc/>
    public override void Register(IServiceCollection services)
    {
        // Register the factory and service
        // The actual registration logic will be handled by the service registration system
        services.AddScoped<IEntraAuthenticationServiceFactory, EntraAuthenticationServiceFactory>();
        services.AddScoped<IAuthenticationService, EntraAuthenticationService>();
    }
}