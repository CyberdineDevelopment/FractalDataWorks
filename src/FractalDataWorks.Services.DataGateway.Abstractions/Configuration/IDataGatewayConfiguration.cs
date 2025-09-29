using FractalDataWorks.Configuration.Abstractions;

namespace FractalDataWorks.Services.DataGateway.Abstractions;

/// <summary>
/// Represents configuration for data gateway services.
/// </summary>
/// <remarks>
/// Data gateway configurations define the settings and parameters needed to establish
/// and manage connections to external data sources through the data gateway abstraction.
/// This includes connection details, caching policies, and operational parameters.
/// </remarks>
public interface IDataGatewayConfiguration : IGenericConfiguration
{
    // Marker interface for data gateway configurations
    // Additional properties specific to data gateway configuration can be added here
}