using System;
using System.Collections.Generic;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Security;
using FractalDataWorks.Services.Authentication.AzureEntra.Configuration;

namespace FractalDataWorks.Services.Authentication.AzureEntra;

/// <summary>
/// Service type definition for Azure Entra (Azure Active Directory) authentication.
/// </summary>
public sealed class AzureEntraAuthenticationServiceType :
    AuthenticationTypeBase<IAuthenticationService, IAuthenticationConfiguration, IAzureEntraAuthenticationServiceFactory>,
    IEnumOption<AzureEntraAuthenticationServiceType>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureEntraAuthenticationServiceType"/> class.
    /// </summary>
    public AzureEntraAuthenticationServiceType()
        : base(id: 1, name: "AzureEntra")
    {
    }

    /// <inheritdoc/>
    public override string ProviderName => "Microsoft.Identity.Client";

    /// <inheritdoc/>
    public override AuthenticationMethod Method => AuthenticationMethod.OAuth2;

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedProtocols => ["OAuth2", "OpenIDConnect", "SAML2"];

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedFlows => ["AuthorizationCode", "ClientCredentials", "DeviceCode", "Interactive", "Silent", "OnBehalfOf"];

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedTokenTypes => ["AccessToken", "IdToken", "RefreshToken"];

    /// <inheritdoc/>
    public override bool SupportsMultiTenant => true;

    /// <inheritdoc/>
    public override bool SupportsTokenCaching => true;

    /// <inheritdoc/>
    public override bool SupportsTokenRefresh => true;

    /// <inheritdoc/>
    public override int Priority => 90;

    /// <inheritdoc/>
    public override Type GetFactoryImplementationType()
    {
        // Use the generic factory for now
        // Can be overridden later to return a custom AzureEntraAuthenticationServiceFactory if needed
        return typeof(GenericServiceFactory<IAuthenticationService, IAuthenticationConfiguration>);
    }
}