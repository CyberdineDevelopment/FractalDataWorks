using System;
using FractalDataWorks.Services.Abstractions.Commands;

namespace FractalDataWorks.Commands.Abstractions;

/// <summary>
/// Base interface for all commands in the FractalDataWorks framework.
/// Commands represent operations that can be executed, translated, and validated.
/// </summary>
/// <remarks>
/// Commands follow the Command Pattern and serve as the foundation for
/// operation abstraction across different implementations (SQL, HTTP, etc.).
/// All commands should be immutable with init-only properties.
/// </remarks>
public interface ICommand : FractalDataWorks.Services.Abstractions.Commands.ICommand
{
    /// <summary>
    /// Gets the unique identifier for this command instance.
    /// </summary>
    /// <value>A unique identifier for tracking and logging purposes.</value>
    Guid CommandId { get; }

    /// <summary>
    /// Gets the timestamp when this command was created.
    /// </summary>
    /// <value>The UTC timestamp of command creation.</value>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Gets the command type name for routing and translation.
    /// </summary>
    /// <value>The type name used by TypeCollections for command routing.</value>
    string CommandType { get; }

    /// <summary>
    /// Gets the command category for classification.
    /// </summary>
    /// <value>The category that determines command behavior and requirements.</value>
    ICommandCategory Category { get; }
}