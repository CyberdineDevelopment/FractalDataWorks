using FractalDataWorks.Services.Authentication.Abstractions;

namespace FractalDataWorks.Services.Authentication.Auth0;

/// <summary>
/// Factory interface for creating Auth0 authentication service instances.
/// </summary>
public interface IAuth0AuthenticationServiceFactory :
    IAuthenticationServiceFactory<IAuthenticationService, IAuthenticationConfiguration>
{
}
