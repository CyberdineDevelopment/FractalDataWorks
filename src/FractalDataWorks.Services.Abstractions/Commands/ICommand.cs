using System;
using FluentValidation.Results;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Services.Abstractions.Commands;

/// <summary>
/// Represents a command that can be executed.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Gets the unique identifier for this command instance.
    /// </summary>
    Guid CommandId { get; }

    /// <summary>
    /// Gets the correlation identifier for tracking related operations.
    /// </summary>
    Guid CorrelationId { get; }

    /// <summary>
    /// Gets the timestamp when this command was created.
    /// </summary>
    DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the configuration associated with this command.
    /// </summary>
    IFdwConfiguration? Configuration { get; }

    /// <summary>
    /// Validates this command.
    /// </summary>
    /// <returns>A FdwResult containing the validation result.</returns>
    IFdwResult<ValidationResult> Validate();
}


/// <summary>
/// Represents a command that can be executed with a payload.
/// </summary>
/// <typeparam name="T">The type of the payload carried by this command.</typeparam>
public interface ICommand<T> : ICommand
{
    /// <summary>
    /// Gets the payload of the command;
    /// </summary>
    T? Payload { get; init; }
}
