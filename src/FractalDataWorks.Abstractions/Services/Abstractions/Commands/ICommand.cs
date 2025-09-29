using System;
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

    // NOTE: Configuration property disabled due to circular dependency issues
    // IGenericConfiguration? Configuration { get; }

    /// <summary>
    /// Validates this command.
    /// </summary>
    /// <returns>A GenericResult containing the validation result.</returns>
    IGenericResult Validate();
}


