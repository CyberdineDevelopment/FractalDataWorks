using FractalDataWorks.Services.Abstractions.Commands;
using System;
using System.Collections.Generic;
using FractalDataWorks.Abstractions;

namespace FractalDataWorks.Services.SecretManagers.Abstractions;


/// <summary>
/// Interface for secret management commands in the FractalDataWorks framework.
/// Represents a managementCommand that can be executed against a secret provider to perform secret operations.
/// </summary>
/// <remarks>
/// Secret commands encapsulate the details of secret operations (get, set, delete, list, etc.)
/// and provide a consistent interface for secret providers to execute operations
/// regardless of the underlying secret storage technology.
/// </remarks>
public interface ISecretManagerCommand : IGenericCommand
{
    /// <summary>
    /// Gets the unique identifier for this managementCommand.
    /// </summary>
    /// <value>A unique identifier for the managementCommand instance.</value>
    /// <remarks>
    /// This identifier is used for managementCommand tracking, logging, and debugging purposes.
    /// It should remain constant for the lifetime of the managementCommand instance.
    /// </remarks>
    new string CommandId { get; }

    /// <summary>
    /// Gets the type of operation this managementCommand represents.
    /// </summary>
    /// <value>The managementCommand type (e.g., "GetSecret", "SetSecret", "DeleteSecret", "ListSecrets", "GetSecretVersions").</value>
    /// <remarks>
    /// ManagementCommand types help secret providers determine how to execute the managementCommand
    /// and what type of result to expect. This enables provider-specific optimizations.
    /// </remarks>
    string CommandType { get; }

    /// <summary>
    /// Gets the target secret container or path for this managementCommand.
    /// </summary>
    /// <value>The target container, vault, or path name, or null if not applicable.</value>
    /// <remarks>
    /// The target helps secret providers route commands to the appropriate storage locations
    /// and apply container-specific configurations or security policies.
    /// </remarks>
    string? Container { get; }

    /// <summary>
    /// Gets the secret key or identifier.
    /// </summary>
    /// <value>The secret key, name, or identifier.</value>
    /// <remarks>
    /// This is the primary identifier for the secret within the specified container.
    /// The format may vary by provider (e.g., hierarchical paths, simple names).
    /// </remarks>
    string? SecretKey { get; }

    /// <summary>
    /// Gets the expected result type for this managementCommand.
    /// </summary>
    /// <value>The Type of object expected to be returned by managementCommand execution.</value>
    /// <remarks>
    /// This information enables secret providers to prepare appropriate result handling
    /// and type conversion logic before executing the managementCommand.
    /// </remarks>
    Type ExpectedResultType { get; }

    /// <summary>
    /// Gets the timeout for managementCommand execution.
    /// </summary>
    /// <value>The maximum time to wait for managementCommand execution, or null for provider default.</value>
    /// <remarks>
    /// ManagementCommand-specific timeouts allow fine-grained control over execution time limits.
    /// If null, the secret provider should use its default timeout configuration.
    /// </remarks>
    TimeSpan? Timeout { get; }

    /// <summary>
    /// Gets the parameters for this managementCommand.
    /// </summary>
    /// <value>A dictionary of parameter names and values for managementCommand execution.</value>
    /// <remarks>
    /// Parameters provide input data for the managementCommand execution. Common parameters include
    /// "SecretValue", "Version", "Tags", "Description", "ExpirationDate".
    /// Parameter names should use consistent naming conventions across commands.
    /// </remarks>
    IReadOnlyDictionary<string, object?> Parameters { get; }

    /// <summary>
    /// Gets additional metadata for this managementCommand.
    /// </summary>
    /// <value>A dictionary of metadata properties that may influence managementCommand execution.</value>
    /// <remarks>
    /// Metadata can include encryption hints, access policies, audit trail requirements,
    /// or other provider-specific configuration options.
    /// Common metadata keys include "EncryptionKey", "AccessPolicy", "AuditEnabled".
    /// </remarks>
    IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets a value indicating whether this managementCommand modifies secrets.
    /// </summary>
    /// <value><c>true</c> if the managementCommand modifies secrets; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// This property helps secret providers determine appropriate access control,
    /// audit logging, and caching behavior for the managementCommand.
    /// </remarks>
    bool IsSecretModifying { get; }

    // Note: Validate() method is inherited from IGenericCommand base interface
    // which returns IGenericResult<ValidationResult>

    /// <summary>
    /// Creates a copy of this managementCommand with modified parameters.
    /// </summary>
    /// <param name="newParameters">The new parameters to use in the copied managementCommand.</param>
    /// <returns>A new managementCommand instance with the specified parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newParameters"/> is null.</exception>
    /// <remarks>
    /// This method enables managementCommand reuse with different parameter sets without
    /// modifying the original managementCommand instance. Useful for batch operations.
    /// </remarks>
    ISecretManagerCommand WithParameters(IReadOnlyDictionary<string, object?> newParameters);

    /// <summary>
    /// Creates a copy of this managementCommand with modified metadata.
    /// </summary>
    /// <param name="newMetadata">The new metadata to use in the copied managementCommand.</param>
    /// <returns>A new managementCommand instance with the specified metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newMetadata"/> is null.</exception>
    /// <remarks>
    /// This method enables managementCommand customization with different execution hints
    /// or configuration options without modifying the original managementCommand instance.
    /// </remarks>
    ISecretManagerCommand WithMetadata(IReadOnlyDictionary<string, object> newMetadata);
}

/// <summary>
/// Generic interface for secret commands with typed result expectations.
/// Extends the base managementCommand interface with compile-time type safety for results.
/// </summary>
/// <typeparam name="TResult">The type of result expected from managementCommand execution.</typeparam>
/// <remarks>
/// Use this interface when the expected result type is known at compile time.
/// It provides type safety and eliminates the need for runtime type checking and casting.
/// </remarks>
public interface ISecretManagerCommand<TResult> : ISecretManagerCommand
{
    /// <summary>
    /// Creates a copy of this managementCommand with modified parameters.
    /// </summary>
    /// <param name="newParameters">The new parameters to use in the copied managementCommand.</param>
    /// <returns>A new typed managementCommand instance with the specified parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newParameters"/> is null.</exception>
    /// <remarks>
    /// This method provides type-safe managementCommand copying for generic managementCommand instances.
    /// </remarks>
    new ISecretManagerCommand<TResult> WithParameters(IReadOnlyDictionary<string, object?> newParameters);

    /// <summary>
    /// Creates a copy of this managementCommand with modified metadata.
    /// </summary>
    /// <param name="newMetadata">The new metadata to use in the copied managementCommand.</param>
    /// <returns>A new typed managementCommand instance with the specified metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newMetadata"/> is null.</exception>
    /// <remarks>
    /// This method provides type-safe managementCommand copying for generic managementCommand instances.
    /// </remarks>
    new ISecretManagerCommand<TResult> WithMetadata(IReadOnlyDictionary<string, object> newMetadata);
}