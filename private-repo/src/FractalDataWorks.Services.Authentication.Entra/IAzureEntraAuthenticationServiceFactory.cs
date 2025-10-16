using FractalDataWorks.Abstractions;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Security;
using FractalDataWorks.Services.Authentication.AzureEntra.Configuration;

namespace FractalDataWorks.Services.Authentication.AzureEntra;

/// <summary>
/// Factory interface for creating Azure Entra authentication service instances.
/// This allows for future custom factory implementations while using GenericServiceFactory by default.
/// </summary>
public interface IEntraAuthenticationServiceFactory : 
    IAuthenticationServiceFactory<IAuthenticationService, IAuthenticationConfiguration>,
    IServiceFactory<IAuthenticationService, IAuthenticationConfiguration>
{
    // No additional members needed - this interface serves as a marker
    // and constraint for the Azure Entra authentication factory.
    // The generic factory will implement this by default.
}