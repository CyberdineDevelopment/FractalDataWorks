using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions.Security;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Concrete collection of all authentication service types in the system.
/// This partial class will be extended by the source generator to include
/// all discovered authentication types with high-performance lookup capabilities.
/// </summary>
[ServiceTypeCollection("AuthenticationTypeBase", "AuthenticationTypes")]
public partial class AuthenticationTypesBase : 
    AuthenticationTypeCollectionBase<
        AuthenticationTypeBase<IAuthenticationService, IAuthenticationConfiguration, IAuthenticationServiceFactory<IAuthenticationService, IAuthenticationConfiguration>>,
        AuthenticationTypeBase<IAuthenticationService, IAuthenticationConfiguration, IAuthenticationServiceFactory<IAuthenticationService, IAuthenticationConfiguration>>,
        IAuthenticationService,
        IAuthenticationConfiguration,
        IAuthenticationServiceFactory<IAuthenticationService, IAuthenticationConfiguration>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationTypesBase"/> class.
    /// The source generator will populate all discovered authentication types.
    /// </summary>
    public AuthenticationTypesBase()
    {
        // Source generator will add:
        // - Static fields for each authentication type (e.g., MicrosoftEntra, Auth0, Okta, etc.)
        // - FrozenDictionary for O(1) lookups by Id/Name
        // - Factory methods for each constructor overload
        // - Empty() method returning default instance
        // - All() method returning all authentication types
        // - Lookup methods by ProviderName, Method, etc.
    }
}