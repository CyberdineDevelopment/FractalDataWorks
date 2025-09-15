using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks;
using FractalDataWorks.Results;
using FluentValidation.Results;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Messages;
using FractalDataWorks.Services.Abstractions.Commands;
using FractalDataWorks.Services.SecretManagement.Abstractions;
using FractalDataWorks.Services.SecretManagement;
using FractalDataWorks.Services.SecretManagement.Commands;
using FractalDataWorks.Services.SecretManagement.Abstractions;
using FractalDataWorks.Services.SecretManagement;

namespace FractalDataWorks.Services.SecretManagement.AzureKeyVault.Commands;

/// <summary>
/// Represents a command for the Azure Key Vault Secret Management service.
/// </summary>
public sealed class AzureKeyVaultCommand : ISecretCommand
{
    private readonly Guid _commandGuid = Guid.NewGuid();
    
    /// <summary>
    /// Gets the unique identifier for this command as a Guid.
    /// </summary>
    Guid ICommand.CommandId => _commandGuid;
    
    /// <summary>
    /// Gets the unique identifier for this command as a string.
    /// </summary>
    string ISecretCommand.CommandId => _commandGuid.ToString();

    /// <summary>
    /// Gets or sets the type of secret operation to perform.
    /// </summary>
    public string CommandType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the secret key to operate on.
    /// </summary>
    public string? SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional parameters for the command.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Parameters { get; set; } = new Dictionary<string, object?>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the container for this command.
    /// </summary>
    public string? Container { get; set; }

    /// <summary>
    /// Gets the expected result type.
    /// </summary>
    public Type ExpectedResultType { get; set; } = typeof(object);

    /// <summary>
    /// Gets the timeout for command execution.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Gets additional metadata for this command.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>(StringComparer.Ordinal);

    /// <summary>
    /// Gets a value indicating whether this command modifies secrets.
    /// Following Enhanced Enum pattern: use ByName() to get behavior from enum option.
    /// </summary>
    public bool IsSecretModifying => 
        EnhancedEnums.SecretCommandTypes.ByName(CommandType).IsSecretModifying;

    /// <summary>
    /// Gets the correlation ID for this command.
    /// </summary>
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp when this command was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the configuration for this command.
    /// </summary>
    public IFdwConfiguration? Configuration { get; set; }

    /// <summary>
    /// Validates the command parameters.
    /// </summary>
    /// <returns>A validation result indicating whether the command is valid.</returns>
    public IFdwResult<ValidationResult> Validate()
    {
        // Basic validation for key and command type
        var validator = new AzureKeyVaultCommandValidator();
        var result = validator.Validate(this);
        
        if (result.IsValid)
        {
            return FdwResult<ValidationResult>.Success(result);
        }
        
        var errorMessage = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
        return FdwResult<ValidationResult>.Failure(new FractalMessage(MessageSeverity.Error, errorMessage, "ValidationFailed", "AzureKeyVaultCommand"));
    }

    /// <summary>
    /// Creates a copy of this command with modified parameters.
    /// </summary>
    public ISecretCommand WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        ArgumentNullException.ThrowIfNull(newParameters);
        
        return new AzureKeyVaultCommand
        {
            CommandType = this.CommandType,
            SecretKey = this.SecretKey,
            Parameters = newParameters,
            Container = this.Container,
            ExpectedResultType = this.ExpectedResultType,
            Timeout = this.Timeout,
            Metadata = this.Metadata,
            CorrelationId = this.CorrelationId,
            Configuration = this.Configuration
        };
    }

    /// <summary>
    /// Creates a copy of this command with modified metadata.
    /// </summary>
    public ISecretCommand WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        ArgumentNullException.ThrowIfNull(newMetadata);
        
        return new AzureKeyVaultCommand
        {
            CommandType = this.CommandType,
            SecretKey = this.SecretKey,
            Parameters = this.Parameters,
            Container = this.Container,
            ExpectedResultType = this.ExpectedResultType,
            Timeout = this.Timeout,
            Metadata = newMetadata,
            CorrelationId = this.CorrelationId,
            Configuration = this.Configuration
        };
    }
}