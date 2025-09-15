using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.SecretManagement.Commands;

/// <summary>
/// Command for retrieving all versions of a specific secret from a secret provider.
/// </summary>
/// <remarks>
/// GetSecretVersionsCommand retrieves version information for all versions of a secret
/// from the configured secret store. This includes version identifiers, creation dates,
/// and other version-specific metadata.
/// </remarks>
public sealed class GetSecretVersionsCommand : SecretCommandBase, ISecretCommand<IEnumerable<SecretValue>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetSecretVersionsCommand"/> class.
    /// </summary>
    /// <param name="container">The secret container or vault name.</param>
    /// <param name="secretKey">The secret key or identifier.</param>
    /// <param name="parameters">Command parameters (e.g., IncludeDisabled, MaxResults).</param>
    /// <param name="metadata">Additional command metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <exception cref="ArgumentException">Thrown when secretKey is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public GetSecretVersionsCommand(
        string? container,
        string secretKey,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("GetSecretVersions", container, secretKey, typeof(IEnumerable<SecretValue>), parameters, metadata, timeout)
    {
    }

    /// <summary>
    /// Creates a GetSecretVersionsCommand for retrieving all versions of a secret.
    /// </summary>
    /// <param name="container">The secret container or vault name.</param>
    /// <param name="secretKey">The secret key to retrieve versions for.</param>
    /// <param name="includeDisabled">Whether to include disabled/deleted versions.</param>
    /// <param name="maxResults">Maximum number of versions to retrieve.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <returns>A new GetSecretVersionsCommand instance.</returns>
    public static GetSecretVersionsCommand Create(
        string? container, 
        string secretKey, 
        bool includeDisabled = false, 
        int? maxResults = null,
        TimeSpan? timeout = null)
    {
        var parameters = new Dictionary<string, object?>();
        
        if (includeDisabled)
            parameters["IncludeDisabled"] = true;
            
        if (maxResults.HasValue)
            parameters["MaxResults"] = maxResults.Value;

        return new GetSecretVersionsCommand(container, secretKey, parameters, null, timeout);
    }

    /// <summary>
    /// Creates a new command with updated parameters while preserving other properties.
    /// </summary>
    /// <param name="parameters">The new parameters dictionary.</param>
    /// <returns>A new GetSecretVersionsCommand instance with updated parameters.</returns>
    public new GetSecretVersionsCommand WithParameters(IReadOnlyDictionary<string, object?> parameters)
    {
        var newParameters = new Dictionary<string, object?>(Parameters);
        foreach (var kvp in parameters)
        {
            newParameters[kvp.Key] = kvp.Value;
        }

        return new GetSecretVersionsCommand(Container, SecretKey!, newParameters, Metadata, Timeout);
    }

    /// <summary>
    /// Creates a new command with updated metadata while preserving other properties.
    /// </summary>
    /// <param name="metadata">The new metadata dictionary.</param>
    /// <returns>A new GetSecretVersionsCommand instance with updated metadata.</returns>
    public new GetSecretVersionsCommand WithMetadata(IReadOnlyDictionary<string, object> metadata)
    {
        var newMetadata = new Dictionary<string, object>(Metadata);
        foreach (var kvp in metadata)
        {
            newMetadata[kvp.Key] = kvp.Value;
        }

        return new GetSecretVersionsCommand(Container, SecretKey!, Parameters, newMetadata, Timeout);
    }

    /// <summary>
    /// Creates a new command with updated timeout while preserving other properties.
    /// </summary>
    /// <param name="timeout">The new timeout value.</param>
    /// <returns>A new GetSecretVersionsCommand instance with updated timeout.</returns>
    public GetSecretVersionsCommand WithTimeout(TimeSpan timeout)
    {
        return new GetSecretVersionsCommand(Container, SecretKey!, Parameters, Metadata, timeout);
    }

    /// <inheritdoc/>
    public override bool IsSecretModifying => false;

    /// <inheritdoc/>
    protected override ISecretCommand CreateCopyWithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        return WithParameters(newParameters);
    }

    /// <inheritdoc/>
    protected override ISecretCommand CreateCopyWithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        return WithMetadata(newMetadata);
    }

    ISecretCommand<IEnumerable<SecretValue>> ISecretCommand<IEnumerable<SecretValue>>.WithParameters(IReadOnlyDictionary<string, object?> parameters)
    {
        return WithParameters(parameters);
    }

    ISecretCommand<IEnumerable<SecretValue>> ISecretCommand<IEnumerable<SecretValue>>.WithMetadata(IReadOnlyDictionary<string, object> metadata)
    {
        return WithMetadata(metadata);
    }
}