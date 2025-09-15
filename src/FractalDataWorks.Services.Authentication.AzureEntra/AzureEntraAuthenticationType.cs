using System;
using System.Collections.Generic;
using FractalDataWorks.Services.Authentication.Abstractions;
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
public sealed class AzureEntraAuthenticationType : 
    AuthenticationTypeBase<IAuthenticationService, IAuthenticationConfiguration, IAzureEntraAuthenticationServiceFactory>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureEntraAuthenticationType"/> class.
    /// </summary>
    public AzureEntraAuthenticationType() 
        : base(1, "AzureEntra", "Authentication")
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
        "interactive",
        "silent",
        "on_behalf_of"
    };

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedTokenTypes => new[]
    {
        "access_token",
        "id_token", 
        "refresh_token"
    };

    /// <inheritdoc/>
    public override bool SupportsMultiTenant => true;

    /// <inheritdoc/>
    public override bool SupportsTokenCaching => true;

    /// <inheritdoc/>
    public override bool SupportsTokenRefresh => true;

    /// <inheritdoc/>
    public override int Priority => 90;

    /// <inheritdoc/>
    public override Type ServiceType => typeof(IAuthenticationService);

    /// <inheritdoc/>
    public override Type ConfigurationType => typeof(IAuthenticationConfiguration);

    /// <inheritdoc/>
    public override Type FactoryType => typeof(IAzureEntraAuthenticationServiceFactory);
}