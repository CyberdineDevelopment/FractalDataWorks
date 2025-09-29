using System;
using System.Collections.Generic;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.DataGateway.Abstractions.Models;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Commands;

/// <summary>
/// Base class for data commands.
/// </summary>
public abstract class DataCommandBase : IDataCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandBase"/> class.
    /// </summary>
    /// <param name="commandName">The name of the command.</param>
    /// <param name="connectionName">The named connection to execute against.</param>
    /// <param name="targetContainer">The target container path.</param>
    /// <param name="parameters">Additional parameters.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    protected DataCommandBase(
        string commandName,
        string connectionName,
        DataPath? targetContainer,
        IReadOnlyDictionary<string, object?>? parameters,
        IReadOnlyDictionary<string, object>? metadata,
        TimeSpan? timeout)
    {
        CommandId = Guid.NewGuid();
        CorrelationId = Guid.NewGuid();
        Timestamp = DateTimeOffset.UtcNow;
        CommandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
        ConnectionName = connectionName;
        TargetContainer = targetContainer;
        Parameters = parameters ?? new Dictionary<string, object?>(StringComparer.Ordinal);
        Metadata = metadata ?? new Dictionary<string, object>(StringComparer.Ordinal);
        Timeout = timeout;
    }

    /// <summary>
    /// Gets the unique identifier for this command instance.
    /// </summary>
    public Guid CommandId { get; }

    /// <summary>
    /// Gets the correlation identifier for tracking related operations.
    /// </summary>
    public Guid CorrelationId { get; }

    /// <summary>
    /// Gets the timestamp when this command was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    public string CommandName { get; }

    /// <summary>
    /// Gets the named connection to execute against.
    /// </summary>
    public string? ConnectionName { get; }

    /// <summary>
    /// Gets the target container path.
    /// </summary>
    public DataPath? TargetContainer { get; }

    /// <summary>
    /// Gets additional parameters for the command.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Parameters { get; }

    /// <summary>
    /// Gets additional metadata for the command.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets the command timeout.
    /// </summary>
    public TimeSpan? Timeout { get; }

    /// <summary>
    /// Gets a value indicating whether this command modifies data.
    /// </summary>
    public abstract bool IsDataModifying { get; }

    /// <summary>
    /// Validates this command.
    /// </summary>
    /// <returns>A FdwResult containing the validation result.</returns>
    public virtual IFdwResult Validate()
    {
        if (string.IsNullOrWhiteSpace(CommandName))
        {
            return FdwResult.Failure("Command name cannot be null or empty");
        }

        return FdwResult.Success();
    }

    /// <summary>
    /// Creates a copy of this command with the specified modifications.
    /// </summary>
    protected abstract DataCommandBase CreateCopy(
        string? connectionName,
        DataPath? targetContainer,
        IReadOnlyDictionary<string, object?> parameters,
        IReadOnlyDictionary<string, object> metadata,
        TimeSpan? timeout);

    /// <summary>
    /// Creates a new command with the specified connection name.
    /// </summary>
    /// <param name="connectionName">The connection name.</param>
    /// <returns>A new command instance with the specified connection name.</returns>
    public DataCommandBase WithConnection(string connectionName)
    {
        return CreateCopy(connectionName, TargetContainer, Parameters, Metadata, Timeout);
    }

    /// <summary>
    /// Creates a new command with the specified timeout.
    /// </summary>
    /// <param name="timeout">The timeout value.</param>
    /// <returns>A new command instance with the specified timeout.</returns>
    public DataCommandBase WithTimeout(TimeSpan timeout)
    {
        return CreateCopy(ConnectionName, TargetContainer, Parameters, Metadata, timeout);
    }
}

/// <summary>
/// Base class for data commands with a result type.
/// </summary>
/// <typeparam name="TResult">The type of the command result.</typeparam>
public abstract class DataCommandBase<TResult> : DataCommandBase, IDataCommand<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandBase{TResult}"/> class.
    /// </summary>
    /// <param name="commandName">The name of the command.</param>
    /// <param name="connectionName">The named connection to execute against.</param>
    /// <param name="targetContainer">The target container path.</param>
    /// <param name="parameters">Additional parameters.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    protected DataCommandBase(
        string commandName,
        string connectionName,
        DataPath? targetContainer,
        IReadOnlyDictionary<string, object?>? parameters,
        IReadOnlyDictionary<string, object>? metadata,
        TimeSpan? timeout)
        : base(commandName, connectionName, targetContainer, parameters, metadata, timeout)
    {
    }
}