using System.Collections.Generic;

namespace FractalDataWorks.Services.Transformations.Abstractions;

/// <summary>
/// Represents the result of a transformation operation.
/// </summary>
public interface ITransformationResult
{
    /// <summary>
    /// Gets the transformed data.
    /// </summary>
    object? Data { get; }
    
    /// <summary>
    /// Gets the output data type.
    /// </summary>
    string OutputType { get; }
    
    /// <summary>
    /// Gets transformation metadata.
    /// </summary>
    IReadOnlyDictionary<string, object?> Metadata { get; }
    
    /// <summary>
    /// Gets the transformation duration in milliseconds.
    /// </summary>
    long DurationMs { get; }
}

/// <summary>
/// Generic interface for transformation results with typed output data.
/// Extends the base transformation result interface with compile-time type safety for output data.
/// </summary>
/// <typeparam name="TOutput">The type of the output data from transformation.</typeparam>
/// <remarks>
/// Use this interface when the output data type is known at compile time.
/// It provides type safety and eliminates the need for runtime type checking and casting.
/// </remarks>
public interface ITransformationResult<TOutput> : ITransformationResult
{
    /// <summary>
    /// Gets the strongly-typed transformed data.
    /// </summary>
    /// <value>The transformed output data.</value>
    new TOutput? Data { get; }
}