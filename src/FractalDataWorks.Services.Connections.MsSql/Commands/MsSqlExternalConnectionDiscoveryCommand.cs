using System;
using System.Linq;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions.Commands;
using FractalDataWorks;
using FluentValidation.Results;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Messages;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.MsSql.Commands;

/// <summary>
/// Command for discovering SQL Server connection schemas and metadata.
/// </summary>
public sealed class MsSqlFdwConnectionDiscoveryCommand : IFdwConnectionCommand, IFdwConnectionDiscoveryCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlFdwConnectionDiscoveryCommand"/> class.
    /// </summary>
    /// <param name="connectionName">The name of the connection to discover.</param>
    /// <param name="startPath">The starting path for schema discovery.</param>
    /// <param name="options">The discovery options.</param>
    public MsSqlFdwConnectionDiscoveryCommand(
        string connectionName, 
        string? startPath = null, 
        ConnectionDiscoveryOptions? options = null)
    {
        ConnectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
        StartPath = startPath;
        Options = options ?? new ConnectionDiscoveryOptions();
    }

    /// <inheritdoc/>
    public string ConnectionName { get; }

    /// <inheritdoc/>
    public string? StartPath { get; }

    /// <inheritdoc/>
    public ConnectionDiscoveryOptions Options { get; }


    /// <summary>
    /// Validates this command using FluentValidation.
    /// </summary>
    /// <returns>The validation result.</returns>
    public IFdwResult<ValidationResult> Validate()
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(ConnectionName))
        {
            result.Errors.Add(new FluentValidation.Results.ValidationFailure(nameof(ConnectionName), "Connection name cannot be null or empty."));
        }

        if (Options.MaxDepth < 0)
        {
            result.Errors.Add(new FluentValidation.Results.ValidationFailure(nameof(Options.MaxDepth), "Max depth cannot be negative."));
        }

        if (result.IsValid)
        {
            return FdwResult<ValidationResult>.Success(result);
        }
        
        var errorMessage = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
        return FdwResult<ValidationResult>.Failure(new FractalMessage(MessageSeverity.Error, errorMessage, "ValidationFailed", "MsSqlFdwConnectionDiscoveryCommand"));
    }

    /// <summary>
    /// Gets the unique identifier for this command instance.
    /// </summary>
    public Guid CommandId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the correlation identifier for tracking related operations.
    /// </summary>
    public Guid CorrelationId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp when this command was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the configuration associated with this command.
    /// </summary>
    public IFractalConfiguration? Configuration => null; // Discovery commands don't have associated configuration
}
