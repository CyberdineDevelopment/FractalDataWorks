using System;
using System.Collections.Generic;
using FractalDataWorks.Services.Abstractions.Commands;
using FractalDataWorks.Services.DataGateway.Abstractions.Models;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Commands;

/// <summary>
/// Represents a command that operates on data through the data gateway.
/// </summary>
/// <remarks>
/// Data commands encapsulate operations against external data sources through
/// the data gateway abstraction. They provide a standardized interface for
/// data operations across different data store types and connection providers.
/// </remarks>
public interface IDataCommand : ICommand
{
    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    string CommandName { get; }

    /// <summary>
    /// Gets the named connection to execute against.
    /// </summary>
    string? ConnectionName { get; }

    /// <summary>
    /// Gets the target container path.
    /// </summary>
    DataPath? TargetContainer { get; }

    /// <summary>
    /// Gets additional parameters for the command.
    /// </summary>
    IReadOnlyDictionary<string, object?> Parameters { get; }

    /// <summary>
    /// Gets additional metadata for the command.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets the command timeout.
    /// </summary>
    TimeSpan? Timeout { get; }
}

/// <summary>
/// Represents a data command with a specific return type.
/// </summary>
/// <typeparam name="TResult">The expected result type of the command.</typeparam>
public interface IDataCommand<TResult> : IDataCommand
{
    // Marker interface for strongly-typed data commands
}