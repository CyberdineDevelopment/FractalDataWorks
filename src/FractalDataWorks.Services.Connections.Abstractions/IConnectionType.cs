using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Interface for connection service types.
/// Defines the contract for connection service type implementations that integrate
/// with the service framework's dependency injection and configuration systems.
/// </summary>
/// <typeparam name="TService">The connection service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the connection service.</typeparam>
/// <typeparam name="TFactory">The factory type for creating connection service instances.</typeparam>
public interface IConnectionType<TService, TConfiguration, TFactory> : IServiceType<TService, TFactory, TConfiguration>
    where TService : IGenericConnection
    where TConfiguration : IConnectionConfiguration
    where TFactory : IConnectionFactory<TService, TConfiguration>
{
    // Connection-specific methods and properties can be added here if needed
}

/// <summary>
/// Non-generic interface for connection service types.
/// Provides a common base for all connection types regardless of generic parameters.
/// </summary>
public interface IConnectionType : IServiceType
{
    // Non-generic connection-specific methods and properties can be added here if needed
}