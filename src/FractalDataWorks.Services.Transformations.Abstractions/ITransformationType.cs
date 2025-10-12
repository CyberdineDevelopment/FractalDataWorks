using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Abstractions;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Data.DataContainers.Abstractions;
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Services.Transformations.Abstractions;

/// <summary>
/// Interface for transformation service types.
/// Defines the contract for transformation service type implementations that integrate
/// with the service framework's dependency injection and configuration systems.
/// </summary>
/// <typeparam name="TService">The transformation service interface type.</typeparam>
/// <typeparam name="TFactory">The factory type for creating transformation service instances.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the transformation service.</typeparam>
public interface ITransformationType<TService, TFactory, TConfiguration> : IServiceType<TService, TFactory, TConfiguration>, ITransformationType
    where TService : class, ITransformationProvider
    where TFactory : class, IServiceFactory<TService, TConfiguration>
    where TConfiguration : class, ITransformationsConfiguration
{
}

/// <summary>
/// Non-generic interface for transformation service types.
/// Provides a common base for all transformation types regardless of generic parameters.
/// </summary>
public interface ITransformationType : IServiceType
{
    /// <summary>
    /// Gets the input type for this transformation.
    /// </summary>
    Type InputType { get; }

    /// <summary>
    /// Gets the output type for this transformation.
    /// </summary>
    Type OutputType { get; }

    /// <summary>
    /// Gets a value indicating whether this transformation supports streaming.
    /// </summary>
    bool SupportsStreaming { get; }

    /// <summary>
    /// Gets a value indicating whether this transformation supports batching.
    /// </summary>
    bool SupportsBatching { get; }

    /// <summary>
    /// Gets a value indicating whether this transformation is reversible.
    /// </summary>
    bool IsReversible { get; }

    /// <summary>
    /// Gets the maximum input size this transformation can handle (in bytes).
    /// </summary>
    long MaxInputSize { get; }

    /// <summary>
    /// Gets the expected performance characteristics for this transformation.
    /// </summary>
    string PerformanceProfile { get; }

    /// <summary>
    /// Gets the memory usage pattern for this transformation.
    /// </summary>
    string MemoryUsagePattern { get; }

    /// <summary>
    /// Gets the supported input formats for this transformation.
    /// </summary>
    IReadOnlyList<string> SupportedInputFormats { get; }

    /// <summary>
    /// Gets the supported output formats for this transformation.
    /// </summary>
    IReadOnlyList<string> SupportedOutputFormats { get; }

    /// <summary>
    /// Gets the data container types that this transformation can work with.
    /// </summary>
    IReadOnlyList<IDataContainerType> SupportedContainerTypes { get; }
}