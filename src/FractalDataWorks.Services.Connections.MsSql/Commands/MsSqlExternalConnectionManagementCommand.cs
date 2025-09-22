using System;
using System.Linq;
using FluentValidation.Results;
using FractalDataWorks.Messages;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions.Commands;
using FractalDataWorks;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.MsSql.Commands;

/// <summary>
/// Command for managing SQL Server connections (list, remove, etc.).
/// </summary>
public sealed class MsSqlFdwConnectionManagementCommand : IConnectionCommand, IConnectionManagementCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlFdwConnectionManagementCommand"/> class.
    /// </summary>
    /// <param name="operation">The management operation to perform.</param>
    /// <param name="connectionName">The connection name (required for some operations).</param>
    public MsSqlFdwConnectionManagementCommand(ConnectionManagementOperation operation, string? connectionName = null)
    {
        Operation = operation;
        ConnectionName = connectionName;
    }

    /// <inheritdoc/>
    public ConnectionManagementOperation Operation { get; }

    /// <inheritdoc/>
    public string? ConnectionName { get; }


    /// <summary>
    /// Validates this command using FluentValidation.
    /// </summary>
    /// <returns>The validation result.</returns>
    public IFdwResult<ValidationResult> Validate()
    {
        var result = new ValidationResult();

        // Operations that require connection name
        if ((Operation == ConnectionManagementOperation.RemoveConnection ||
             Operation == ConnectionManagementOperation.GetConnectionMetadata ||
             Operation == ConnectionManagementOperation.RefreshConnectionStatus) &&
            string.IsNullOrWhiteSpace(ConnectionName))
        {
            result.Errors.Add(new FluentValidation.Results.ValidationFailure(nameof(ConnectionName), $"Connection name is required for {Operation} operation."));
        }

        if (result.IsValid)
        {
            return FdwResult<ValidationResult>.Success(result);
        }
        
        var errorMessage = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
        return FdwResult<ValidationResult>.Failure(new FractalMessage(MessageSeverity.Error, errorMessage, "ValidationFailed", "MsSqlFdwConnectionManagementCommand"));
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
    public IFdwConfiguration? Configuration => null; // Management commands don't have associated configuration
}
