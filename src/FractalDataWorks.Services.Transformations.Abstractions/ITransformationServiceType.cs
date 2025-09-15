using System;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Transformations.Abstractions;

/// <summary>
/// Interface that must be implemented by transformation service type definitions.
/// Provides metadata specific to transformation services.
/// </summary>
public interface ITransformationServiceType : IServiceType
{
    /// <summary>
    /// Gets the supported input data types for this transformation service.
    /// </summary>
    string[] SupportedInputTypes { get; }

    /// <summary>
    /// Gets the supported output data types for this transformation service.
    /// </summary>
    string[] SupportedOutputTypes { get; }

    /// <summary>
    /// Gets the supported transformation categories.
    /// </summary>
    string[] SupportedCategories { get; }

    /// <summary>
    /// Gets a value indicating whether this service supports parallel transformations.
    /// </summary>
    bool SupportsParallelExecution { get; }

    /// <summary>
    /// Gets a value indicating whether this service supports transformation caching.
    /// </summary>
    bool SupportsTransformationCaching { get; }

    /// <summary>
    /// Gets a value indicating whether this service supports transformation pipelines.
    /// </summary>
    bool SupportsPipelineMode { get; }

    /// <summary>
    /// Gets the maximum input size in bytes this service can handle.
    /// </summary>
    long MaxInputSizeBytes { get; }

    /// <summary>
    /// Gets the priority for this transformation service provider.
    /// </summary>
    int Priority { get; }
}