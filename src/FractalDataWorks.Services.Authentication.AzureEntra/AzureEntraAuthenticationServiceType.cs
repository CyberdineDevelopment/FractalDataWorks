using System;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Security;
using FractalDataWorks.Services.Authentication.AzureEntra.Configuration;

namespace FractalDataWorks.Services.Authentication.AzureEntra;

/// <summary>
/// Service type definition for Azure Entra (Azure Active Directory) authentication.
/// </summary>
[EnumOption("AzureEntra")]
public sealed class AzureEntraAuthenticationServiceType : 
    AuthenticationServiceType<AzureEntraAuthenticationServiceType, IAuthenticationService, IAuthenticationConfiguration, IAzureEntraAuthenticationServiceFactory>,
    IEnumOption<AzureEntraAuthenticationServiceType>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureEntraAuthenticationServiceType"/> class.
    /// </summary>
    public AzureEntraAuthenticationServiceType() 
        : base(
            id: 1, 
            name: "AzureEntra", 
            description: "Azure Entra ID (Azure Active Directory) authentication service",
            providerName: "Microsoft.Identity.Client",
            supportedProtocols: ["OAuth2", "OpenIDConnect", "SAML2"],
            supportedFlows: ["AuthorizationCode", "ClientCredentials", "DeviceCode", "Interactive", "Silent", "OnBehalfOf"
            ],
            supportedTokenTypes: ["AccessToken", "IdToken", "RefreshToken"],
            priority: 90,
            supportsMultiTenant: true,
            supportsTokenCaching: true)
    {
    }

    /// <inheritdoc/>
    public override Type GetFactoryImplementationType()
    {
        // Use the generic factory for now
        // Can be overridden later to return a custom AzureEntraAuthenticationServiceFactory if needed
        return typeof(GenericServiceFactory<IAuthenticationService, IAuthenticationConfiguration>);
    }
}