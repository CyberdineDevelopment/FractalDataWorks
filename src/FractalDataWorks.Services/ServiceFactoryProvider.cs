using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Abstractions;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services;

/// <summary>
/// Default implementation of IServiceFactoryProvider that manages service factory registrations.
/// </summary>
/// <remarks>
/// This implementation provides thread-safe registration and retrieval of service factories.
/// Factories are stored by their service type name and can be retrieved for dynamic service creation.
/// </remarks>
public sealed class ServiceFactoryProvider : IServiceFactoryProvider
{
    private readonly Dictionary<string, FactoryRegistration> _registrations;
    private readonly object _lock;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceFactoryProvider"/> class.
    /// </summary>
    public ServiceFactoryProvider()
    {
        _registrations = new Dictionary<string, FactoryRegistration>(StringComparer.OrdinalIgnoreCase);
        _lock = new object();
    }

    /// <inheritdoc/>
    public IGenericResult RegisterFactory(string typeName, IServiceFactory factory)
    {
        // Use default Scoped lifetime for backward compatibility
        return RegisterFactory(typeName, factory, ServiceLifetimes.Scoped);
    }

    /// <summary>
    /// Registers a factory for a specific service type with the specified lifetime.
    /// </summary>
    /// <param name="typeName">The service type name to register the factory for.</param>
    /// <param name="factory">The factory instance to register.</param>
    /// <param name="lifetime">The service lifetime for DI container registration.</param>
    /// <returns>A result indicating success or failure.</returns>
    public IGenericResult RegisterFactory(string typeName, IServiceFactory factory, IServiceLifetime lifetime)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return GenericResult.Failure("Service type name cannot be null or empty");

        if (factory == null)
            return GenericResult.Failure("Factory cannot be null");

        if (lifetime == null)
            return GenericResult.Failure("Lifetime cannot be null");

        lock (_lock)
        {
            if (_registrations.ContainsKey(typeName))
            {
                return GenericResult.Failure($"Factory for service type '{typeName}' is already registered");
            }

            _registrations[typeName] = new FactoryRegistration
            {
                Factory = factory,
                Lifetime = lifetime,
                TypeName = typeName
            };
        }

        return GenericResult.Success();
    }

    /// <inheritdoc/>
    public IGenericResult<IServiceFactory> GetFactory(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return GenericResult<IServiceFactory>.Failure("Service type name cannot be null or empty");

        lock (_lock)
        {
            if (_registrations.TryGetValue(typeName, out var registration))
            {
                return GenericResult<IServiceFactory>.Success(registration.Factory);
            }
        }

        var availableTypes = string.Join(", ", GetRegisteredTypeNames());
        return GenericResult<IServiceFactory>.Failure(
            $"No factory registered for service type '{typeName}'. Available types: {availableTypes}");
    }

    /// <inheritdoc/>
    public IGenericResult<IServiceFactory<TService, TConfiguration>> GetFactory<TService, TConfiguration>(string typeName)
        where TService : class
        where TConfiguration : IGenericConfiguration
    {
        var factoryResult = GetFactory(typeName);
        if (factoryResult.IsFailure)
        {
            return GenericResult<IServiceFactory<TService, TConfiguration>>.Failure(factoryResult.CurrentMessage ?? "Failed to get factory");
        }

        if (factoryResult.Value is IServiceFactory<TService, TConfiguration> typedFactory)
        {
            return GenericResult<IServiceFactory<TService, TConfiguration>>.Success(typedFactory);
        }

        return GenericResult<IServiceFactory<TService, TConfiguration>>.Failure(
            $"Factory for service type '{typeName}' does not implement IServiceFactory<{typeof(TService).Name}, {typeof(TConfiguration).Name}>");
    }

    /// <inheritdoc/>
    public bool IsRegistered(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return false;

        lock (_lock)
        {
            return _registrations.ContainsKey(typeName);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetRegisteredTypeNames()
    {
        lock (_lock)
        {
            return _registrations.Keys.ToList(); // Return a copy to avoid concurrent modification issues
        }
    }

    /// <summary>
    /// Gets the factory registration for a specific service type.
    /// </summary>
    /// <param name="typeName">The service type name to get the registration for.</param>
    /// <returns>A result containing the factory registration or an error.</returns>
    /// <remarks>
    /// This method provides access to the complete registration information including
    /// factory, lifetime, and metadata. Useful for DI container configuration.
    /// </remarks>
    public IGenericResult<FactoryRegistration> GetRegistration(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return GenericResult<FactoryRegistration>.Failure("Service type name cannot be null or empty");

        lock (_lock)
        {
            if (_registrations.TryGetValue(typeName, out var registration))
            {
                return GenericResult<FactoryRegistration>.Success(registration);
            }
        }

        var availableTypes = string.Join(", ", GetRegisteredTypeNames());
        return GenericResult<FactoryRegistration>.Failure(
            $"No registration found for service type '{typeName}'. Available types: {availableTypes}");
    }

    /// <summary>
    /// Gets all factory registrations.
    /// </summary>
    /// <returns>A collection of all factory registrations.</returns>
    /// <remarks>
    /// Returns a copy of all registrations to prevent concurrent modification issues.
    /// Useful for bulk DI container configuration or diagnostic purposes.
    /// </remarks>
    public IEnumerable<FactoryRegistration> GetAllRegistrations()
    {
        lock (_lock)
        {
            return _registrations.Values.ToList(); // Return a copy
        }
    }
}