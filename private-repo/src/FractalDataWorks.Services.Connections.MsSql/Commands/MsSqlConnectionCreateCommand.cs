using System;
using System.Linq;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions.Commands;
using FractalDataWorks.Services.Connections.Abstractions.Messages;
using FractalDataWorks;
using FractalDataWorks.Messages;
using FractalDataWorks.Results;
using FluentValidation.Results;
using FractalDataWorks.Configuration.Abstractions;

namespace FractalDataWorks.Services.Connections.MsSql.Commands;

/// <summary>
/// Command for creating new SQL Server connections.
/// </summary>
public sealed class MsSqlConnectionCreateCommand : IConnectionCommand, IConnectionCreateCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlConnectionCreateCommand"/> class.
    /// </summary>
    /// <param name="connectionName">The name for the new connection.</param>
    /// <param name="connectionConfiguration">The SQL Server configuration for the connection.</param>
    public MsSqlConnectionCreateCommand(string connectionName, MsSqlConfiguration connectionConfiguration)
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
    public IGenericResult Validate()
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
            return GenericResult.Success();
        }

        var errors = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
        return GenericResult.Failure(ConnectionMessages.ValidationFailed(errors));
    }

    /// <summary>
    /// Gets the unique identifier for this command instance.
    /// </summary>
    public Guid CommandId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp when this command was created.
    /// </summary>
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the command type identifier.
    /// </summary>
    public string CommandType => "ConnectionCreate";
}
