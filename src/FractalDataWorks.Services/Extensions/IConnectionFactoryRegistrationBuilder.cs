using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Abstractions;
using static FractalDataWorks.Services.Abstractions.ServiceLifetimes;

namespace FractalDataWorks.Services.Extensions;

/// <summary>
/// Builder interface for fluent configuration of connection factory registrations.
/// </summary>
public interface IConnectionFactoryRegistrationBuilder
{
    /// <summary>
    /// Registers a connection factory with the specified type name.
    /// </summary>
    /// <typeparam name="TFactory">The factory type to register.</typeparam>
    /// <param name="typeName">The service type name to register the factory for.</param>
    /// <param name="lifetime">The service lifetime for the factory (default: Scoped).</param>
    /// <returns>The builder for chaining.</returns>
    IConnectionFactoryRegistrationBuilder RegisterFactory<TFactory>(
        string typeName,
        IServiceLifetime? lifetime = null)
        where TFactory : class, IServiceFactory;

    /// <summary>
    /// Registers a connection factory instance with the specified type name.
    /// </summary>
    /// <param name="typeName">The service type name to register the factory for.</param>
    /// <param name="factory">The factory instance to register.</param>
    /// <returns>The builder for chaining.</returns>
    IConnectionFactoryRegistrationBuilder RegisterFactory(string typeName, IServiceFactory factory);
}