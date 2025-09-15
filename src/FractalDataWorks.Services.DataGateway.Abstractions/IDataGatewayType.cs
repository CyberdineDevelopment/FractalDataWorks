using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.DataGateway.Abstractions;

/// <summary>
/// Interface for data provider service types.
/// Defines the contract for data provider service type implementations that integrate
/// with the service framework's dependency injection and configuration systems.
/// </summary>
/// <typeparam name="TService">The data provider service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the data provider service.</typeparam>
/// <typeparam name="TFactory">The factory type for creating data provider service instances.</typeparam>
public interface IDataGatewayType<TService, TConfiguration, TFactory> : IServiceType<TService, TConfiguration, TFactory>
    where TService : class, IDataService, IFractalService
    where TConfiguration : class, IDataGatewaysConfiguration, IFractalConfiguration
    where TFactory : class, IServiceFactory<TService, TConfiguration>
{
    /// <summary>
    /// Gets the data store types supported by this provider.
    /// </summary>
    string[] SupportedDataStores { get; }

    /// <summary>
    /// Gets the priority for provider selection when multiple providers are available.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports database transactions.
    /// </summary>
    bool SupportsTransactions { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports batch operations.
    /// </summary>
    bool SupportsBatchOperations { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports streaming operations.
    /// </summary>
    bool SupportsStreaming { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports connection pooling.
    /// </summary>
    bool SupportsConnectionPooling { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports schema discovery.
    /// </summary>
    bool SupportsSchemaDiscovery { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports data caching.
    /// </summary>
    bool SupportsDataCaching { get; }

    /// <summary>
    /// Gets the maximum batch size supported by this provider.
    /// </summary>
    int MaxBatchSize { get; }

    /// <summary>
    /// Gets the connection timeout in seconds for this provider.
    /// </summary>
    int ConnectionTimeoutSeconds { get; }

    /// <summary>
    /// Gets the query timeout in seconds for this provider.
    /// </summary>
    int QueryTimeoutSeconds { get; }
}

/// <summary>
/// Non-generic interface for data provider service types.
/// Provides a common base for all data provider types regardless of generic parameters.
/// </summary>
public interface IDataGatewayType : IServiceType
{
    /// <summary>
    /// Gets the data store types supported by this provider.
    /// </summary>
    string[] SupportedDataStores { get; }

    /// <summary>
    /// Gets the priority for provider selection when multiple providers are available.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports database transactions.
    /// </summary>
    bool SupportsTransactions { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports batch operations.
    /// </summary>
    bool SupportsBatchOperations { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports streaming operations.
    /// </summary>
    bool SupportsStreaming { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports connection pooling.
    /// </summary>
    bool SupportsConnectionPooling { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports schema discovery.
    /// </summary>
    bool SupportsSchemaDiscovery { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports data caching.
    /// </summary>
    bool SupportsDataCaching { get; }

    /// <summary>
    /// Gets the maximum batch size supported by this provider.
    /// </summary>
    int MaxBatchSize { get; }

    /// <summary>
    /// Gets the connection timeout in seconds for this provider.
    /// </summary>
    int ConnectionTimeoutSeconds { get; }

    /// <summary>
    /// Gets the query timeout in seconds for this provider.
    /// </summary>
    int QueryTimeoutSeconds { get; }
}