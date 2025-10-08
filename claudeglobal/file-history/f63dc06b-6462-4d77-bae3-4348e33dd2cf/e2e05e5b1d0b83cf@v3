using System;
using System.Linq;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions.Commands;
using FractalDataWorks;
using FractalDataWorks.Messages;
using FractalDataWorks.Results;
using FluentValidation.Results;
using FractalDataWorks.Configuration.Abstractions;

namespace FractalDataWorks.Services.Connections.MsSql.Commands;

/// <summary>
/// Command for creating new SQL Server connections.
/// </summary>
public sealed class MsSqlGenericConnectionCreateCommand : IConnectionCommand, IConnectionCreateCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlGenericConnectionCreateCommand"/> class.
    /// </summary>
    /// <param name="connectionName">The name for the new connection.</param>
    /// <param name="connectionConfiguration">The SQL Server configuration for the connection.</param>
    public MsSqlGenericConnectionCreateCommand(string connectionName, MsSqlConfiguration connectionConfiguration)
    {
        ConnectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
        ConnectionConfiguration = connectionConfiguration ?? throw new ArgumentNullException(nameof(connectionConfiguration));
    }

    /// <inheritdoc/>
    public string ConnectionName { get; }

    /// <inheritdoc/>
    public string ProviderType => nameof(MsSql);

    /// <inheritdoc/>
    public IConnectionConfiguration ConnectionConfiguration { get; }


    /// <summary>
    /// Validates this command using FluentValidation.
    /// </summary>
    /// <returns>The validation result.</returns>
    public IGenericResult<ValidationResult> Validate()
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(ConnectionName))
        {
            result.Errors.Add(new FluentValidation.Results.ValidationFailure(nameof(ConnectionName), "Connection name cannot be null or empty."));
        }

        if (ConnectionConfiguration is not MsSqlConfiguration)
        {
            result.Errors.Add(new FluentValidation.Results.ValidationFailure(nameof(ConnectionConfiguration), "Connection configuration must be MsSqlConfiguration."));
        }

        if (result.IsValid)
        {
            return GenericResult<ValidationResult>.Success(result);
        }
        
        var errors = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
        return GenericResult<ValidationResult>.Failure(new FractalMessage(MessageSeverity.Error, errors, "ValidationFailed", "MsSqlGenericConnectionCreateCommand"));
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
    public IGenericConfiguration? Configuration => ConnectionConfiguration as IGenericConfiguration;
}
