using FractalDataWorks.Abstractions;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Security;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Non-generic marker interface for authentication service factories.
/// </summary>
public interface IAuthenticationServiceFactory : IServiceFactory
{
}

/// <summary>
/// Interface for authentication service factories that create specific authentication service implementations.
/// </summary>
/// <typeparam name="TAuthenticationService">The authentication service type to create.</typeparam>
public interface IAuthenticationServiceFactory<TAuthenticationService> : IAuthenticationServiceFactory, IServiceFactory<TAuthenticationService>
    where TAuthenticationService : class, IAuthenticationService
{
}

/// <summary>
/// Interface for authentication service factories that create authentication services with configuration.
/// </summary>
/// <typeparam name="TAuthenticationService">The authentication service type to create.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the authentication service.</typeparam>
public interface IAuthenticationServiceFactory<TAuthenticationService, TConfiguration> : IAuthenticationServiceFactory<TAuthenticationService>, IServiceFactory<TAuthenticationService, TConfiguration>
    where TAuthenticationService : class, IAuthenticationService
    where TConfiguration : class, IAuthenticationConfiguration
{
}