using System;
using System.Collections.Generic;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services;

/// <summary>
/// Provides a registry for service factories that can be retrieved by service type name.
/// </summary>
/// <remarks>
/// The ServiceFactoryProvider acts as a centralized registry where different service types
/// can register their factories, and consumers can retrieve the appropriate factory by name.
/// This enables dynamic service creation based on configuration without tight coupling.
/// </remarks>
public interface IServiceFactoryProvider
{
    /// <summary>
    /// Registers a service factory for the specified service type name.
    /// </summary>
    /// <param name="typeName">The service type name to register the factory for.</param>
    /// <param name="factory">The factory instance to register.</param>
    /// <returns>A result indicating success or failure of the registration.</returns>
    /// <remarks>
    /// The type name should match the Name property of the corresponding ServiceTypeBase
    /// implementation. For example, "MsSql", "Rest", "GraphQL", etc.
    /// </remarks>
    IFdwResult RegisterFactory(string typeName, IServiceFactory factory);

    /// <summary>
    /// Registers a factory for a specific service type with the specified lifetime.
    /// </summary>
    /// <param name="typeName">The service type name to register the factory for.</param>
    /// <param name="factory">The factory instance to register.</param>
    /// <param name="lifetime">The service lifetime for DI container registration.</param>
    /// <returns>A result indicating success or failure.</returns>
    IFdwResult RegisterFactory(string typeName, IServiceFactory factory, IServiceLifetime lifetime);

    /// <summary>
    /// Gets the service factory for the specified service type name.
    /// </summary>
    /// <param name="typeName">The service type name to get the factory for.</param>
    /// <returns>A result containing the factory, or a failure if not found.</returns>
    /// <remarks>
    /// This method is used by service providers to retrieve the appropriate factory
    /// for creating services based on configuration values.
    /// </remarks>
    IFdwResult<IServiceFactory> GetFactory(string typeName);

    /// <summary>
    /// Gets the service factory for the specified service type, with generic type safety.
    /// </summary>
    /// <typeparam name="TService">The service interface type.</typeparam>
    /// <typeparam name="TConfiguration">The configuration type.</typeparam>
    /// <param name="typeName">The service type name to get the factory for.</param>
    /// <returns>A result containing the strongly-typed factory, or a failure if not found.</returns>
    IFdwResult<IServiceFactory<TService, TConfiguration>> GetFactory<TService, TConfiguration>(string typeName)
        where TService : class
        where TConfiguration : IFdwConfiguration;

    /// <summary>
    /// Checks whether a factory is registered for the specified service type name.
    /// </summary>
    /// <param name="typeName">The service type name to check.</param>
    /// <returns>True if a factory is registered; otherwise, false.</returns>
    bool IsRegistered(string typeName);

    /// <summary>
    /// Gets all registered service type names.
    /// </summary>
    /// <returns>An enumerable of all registered type names.</returns>
    IEnumerable<string> GetRegisteredTypeNames();
}