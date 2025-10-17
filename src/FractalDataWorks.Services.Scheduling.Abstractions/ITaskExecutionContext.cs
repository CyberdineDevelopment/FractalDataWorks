using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Provides context information and services for task execution.
/// </summary>
public interface ITaskExecutionContext
{
    /// <summary>
    /// Gets the unique identifier for the current task execution.
    /// </summary>
    string ExecutionId { get; }

    /// <summary>
    /// Gets the cancellation token for the current execution.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets the logger for the current execution context.
    /// </summary>
    ILogger Logger { get; }

    /// <summary>
    /// Gets the service provider for dependency resolution.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the execution start time.
    /// </summary>
    DateTimeOffset StartTime { get; }

    /// <summary>
    /// Gets or sets custom properties for the execution context.
    /// </summary>
    IReadOnlyDictionary<string, object> Properties { get; }

    /// <summary>
    /// Gets the maximum execution time allowed for the task.
    /// </summary>
    TimeSpan? MaxExecutionTime { get; }

    /// <summary>
    /// Sets a custom property for the execution context.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="value">The property value.</param>
    void SetProperty(string key, object value);

    /// <summary>
    /// Gets a custom property value by key.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="key">The property key.</param>
    /// <returns>The property value, or default if not found.</returns>
    T? GetProperty<T>(string key);
}