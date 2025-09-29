using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FractalDataWorks.Results;
using FractalDataWorks.Services.SecretManagers.Abstractions;
using FractalDataWorks.Services.SecretManager;
using FractalDataWorks.Services.SecretManagers.Commands;
using FractalDataWorks.Services.SecretManagers.Abstractions;
using FractalDataWorks.Services.SecretManager;
using FractalDataWorks.Services.SecretManagers.AzureKeyVault.Configuration;
using FractalDataWorks.Services.SecretManagers.AzureKeyVault.EnhancedEnums;
using FractalDataWorks.Services.SecretManagers.AzureKeyVault.Logging;
using Microsoft.Extensions.Logging;
using ISecretManagerCommand = FractalDataWorks.Services.SecretManagers.Commands.ISecretManagerCommand;

namespace FractalDataWorks.Services.SecretManagers.AzureKeyVault;

/// <summary>
/// Azure Key Vault implementation of the secret management service.
/// </summary>
/// <remarks>
/// Provides secure secret storage and retrieval using Microsoft Azure Key Vault.
/// Supports multiple authentication methods including managed identity, service principal,
/// and certificate-based authentication.
/// </remarks>
public sealed class AzureKeyVaultService : SecretManagerServiceBase<ISecretManagerCommand, AzureKeyVaultConfiguration, AzureKeyVaultService>
{
    private readonly SecretClient _secretClient;
    private readonly AzureKeyVaultConfigurationValidator _configurationValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The Azure Key Vault configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or configuration is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid.</exception>
    public AzureKeyVaultService(ILogger<AzureKeyVaultService> logger, AzureKeyVaultConfiguration configuration)
        : base(logger, configuration)
    {
        _configurationValidator = new AzureKeyVaultConfigurationValidator();
        
        ValidateConfiguration();
        _secretClient = CreateSecretClient();
    }

    /// <inheritdoc/>
    public override async Task<IGenericResult> Execute(ISecretManagerCommand managementCommand, CancellationToken cancellationToken)
    {
        if (managementCommand == null)
            return GenericResult.Failure("ManagementCommand cannot be null.");

        try
        {
            AzureKeyVaultServiceLog.ExecutingCommand(Logger, managementCommand.CommandType, managementCommand.CommandId);

            // Get managementCommand type using Enhanced Enum
            var commandType = SecretCommandTypeCollectionBase.All().FirstOrDefault(ct => ct.Name == managementCommand.CommandType);
            if (commandType == null)
            {
                return GenericResult.Failure($"Unsupported managementCommand type: {managementCommand.CommandType}");
            }

            // Validate managementCommand before execution
            if (!commandType.Validate(managementCommand))
            {
                return GenericResult.Failure($"ManagementCommand validation failed for {managementCommand.CommandType}");
            }

            // Execute using Enhanced Enum pattern
            var result = await commandType.Execute(this, managementCommand, cancellationToken).ConfigureAwait(false);

            AzureKeyVaultServiceLog.CommandCompleted(Logger, managementCommand.CommandId, result.IsSuccess);

            return result;
        }
        catch (RequestFailedException ex)
        {
            AzureKeyVaultServiceLog.AzureRequestFailed(Logger, managementCommand.CommandId, ex.ErrorCode, ex.Message, ex);
            
            return GenericResult.Failure($"Azure Key Vault error: {ex.Message}");
        }
        catch (Exception ex)
        {
            AzureKeyVaultServiceLog.UnexpectedError(Logger, managementCommand.CommandId, ex);
            return GenericResult.Failure($"Unexpected error: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public override async Task<IGenericResult<TOut>> Execute<TOut>(ISecretManagerCommand managementCommand, CancellationToken cancellationToken)
    {
        if (managementCommand == null)
            return GenericResult<TOut>.Failure("ManagementCommand cannot be null.");

        var validationResult = managementCommand.Validate();
        if (!validationResult.Value.IsValid)
        {
            var errors = string.Join("; ", validationResult.Value.Errors.Select(e => e.ErrorMessage));
            return GenericResult<TOut>.Failure($"ManagementCommand validation failed: {errors}");
        }

        try
        {
            AzureKeyVaultServiceLog.ExecutingCommand(Logger, managementCommand.CommandType, managementCommand.CommandId);

            // Get managementCommand type using Enhanced Enum
            var commandType = SecretCommandTypeCollectionBase.All().FirstOrDefault(ct => ct.Name == managementCommand.CommandType);
            if (commandType == null)
            {
                return GenericResult<TOut>.Failure($"Unsupported managementCommand type: {managementCommand.CommandType}");
            }

            // Validate managementCommand before execution
            if (!commandType.Validate(managementCommand))
            {
                return GenericResult<TOut>.Failure($"ManagementCommand validation failed for {managementCommand.CommandType}");
            }

            // Execute using Enhanced Enum pattern
            var result = await commandType.Execute(this, managementCommand, cancellationToken).ConfigureAwait(false);

            AzureKeyVaultServiceLog.CommandCompleted(Logger, managementCommand.CommandId, result.IsSuccess);
            
            if (!result.IsSuccess)
            {
                return GenericResult<TOut>.Failure(result.Message ?? "ManagementCommand execution failed");
            }

            // For non-generic results, we need to handle the return differently
            // This is a design issue that needs architectural review
            return GenericResult<TOut>.Success(default!);
        }
        catch (RequestFailedException ex)
        {
            AzureKeyVaultServiceLog.AzureRequestFailed(Logger, managementCommand.CommandId, ex.ErrorCode, ex.Message, ex);
            
            return GenericResult<TOut>.Failure($"Azure Key Vault error: {ex.Message}");
        }
        catch (Exception ex)
        {
            AzureKeyVaultServiceLog.UnexpectedError(Logger, managementCommand.CommandId, ex);
            return GenericResult<TOut>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public override async Task<IGenericResult<T>> Execute<T>(ISecretManagerCommand managementCommand)
    {
        return await Execute<T>(managementCommand, CancellationToken.None).ConfigureAwait(false);
    }


    private async Task<IGenericResult> ExecuteGetSecret(ISecretManagerCommand managementCommand, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(managementCommand.SecretKey))
            return GenericResult.Failure("SecretKey is required for GetSecret operation.");

        try
        {
            var secretName = SanitizeSecretName(managementCommand.SecretKey);
            
            // Check if a specific version is requested
            var version = managementCommand.Parameters.TryGetValue(nameof(Version), out var versionObj) ? 
                versionObj?.ToString() : null;

            KeyVaultSecret secret;
            if (!string.IsNullOrWhiteSpace(version))
            {
                secret = await _secretClient.GetSecretAsync(secretName, version, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                secret = await _secretClient.GetSecretAsync(secretName, null, cancellationToken).ConfigureAwait(false);
            }

            var includeMetadata = managementCommand.Parameters.TryGetValue("IncludeMetadata", out var includeObj) && 
                                 includeObj is bool include && include;

            var secretValue = CreateSecretValue(secret, includeMetadata);
            
            AzureKeyVaultServiceLog.SecretRetrieved(Logger, secretName, secret.Properties.Version);

            return GenericResult.Success();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            AzureKeyVaultServiceLog.SecretNotFound(Logger, managementCommand.SecretKey);
            return GenericResult.Failure($"Secret '{managementCommand.SecretKey}' not found.");
        }
    }

    private async Task<IGenericResult> ExecuteSetSecret(ISecretManagerCommand managementCommand, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(managementCommand.SecretKey))
            return GenericResult.Failure("SecretKey is required for SetSecret operation.");

        if (!managementCommand.Parameters.TryGetValue(nameof(SecretValue), out var secretValueObj) || 
            secretValueObj?.ToString() is not string secretValue)
        {
            return GenericResult.Failure("SecretValue parameter is required for SetSecret operation.");
        }

        try
        {
            var secretName = SanitizeSecretName(managementCommand.SecretKey);
            var secretOptions = new KeyVaultSecret(secretName, secretValue);

            // Apply optional parameters
            if (managementCommand.Parameters.TryGetValue("Description", out var descObj) && 
                descObj?.ToString() is string description)
            {
                secretOptions.Properties.ContentType = description;
            }

            if (managementCommand.Parameters.TryGetValue("ExpirationDate", out var expiryObj) && 
                expiryObj is DateTimeOffset expirationDate)
            {
                secretOptions.Properties.ExpiresOn = expirationDate;
            }

            if (managementCommand.Parameters.TryGetValue("Tags", out var tagsObj) && 
                tagsObj is IReadOnlyDictionary<string, string> tags)
            {
                foreach (var tag in tags)
                {
                    secretOptions.Properties.Tags[tag.Key] = tag.Value;
                }
            }

            var response = await _secretClient.SetSecretAsync(secretOptions, cancellationToken).ConfigureAwait(false);
            var resultValue = CreateSecretValue(response.Value, true);
            
            AzureKeyVaultServiceLog.SecretSet(Logger, secretName, response.Value.Properties.Version);

            return GenericResult.Success();
        }
        catch (RequestFailedException ex) when (ex.Status == 403)
        {
            AzureKeyVaultServiceLog.SecretSetAccessDenied(Logger, managementCommand.SecretKey, ex.Message);
            return GenericResult.Failure($"Access denied setting secret '{managementCommand.SecretKey}': {ex.Message}");
        }
    }

    private async Task<IGenericResult> ExecuteDeleteSecret(ISecretManagerCommand managementCommand, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(managementCommand.SecretKey))
            return GenericResult.Failure("SecretKey is required for DeleteSecret operation.");

        try
        {
            var secretName = SanitizeSecretName(managementCommand.SecretKey);
            var isPermanent = managementCommand.Parameters.TryGetValue("PermanentDelete", out var permanentObj) && 
                             permanentObj is bool permanent && permanent;

            if (isPermanent)
            {
                // First delete, then purge
                await _secretClient.StartDeleteSecretAsync(secretName, cancellationToken).ConfigureAwait(false);
                
                // Wait a bit for the delete to process
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
                
                await _secretClient.PurgeDeletedSecretAsync(secretName, cancellationToken).ConfigureAwait(false);
                
                AzureKeyVaultServiceLog.SecretPurged(Logger, secretName);
            }
            else
            {
                await _secretClient.StartDeleteSecretAsync(secretName, cancellationToken).ConfigureAwait(false);
                
                AzureKeyVaultServiceLog.SecretDeleted(Logger, secretName);
            }

            return GenericResult.Success();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            AzureKeyVaultServiceLog.SecretNotFoundForDeletion(Logger, managementCommand.SecretKey);
            return GenericResult.Failure($"Secret '{managementCommand.SecretKey}' not found.");
        }
        catch (RequestFailedException ex) when (ex.Status == 403)
        {
            AzureKeyVaultServiceLog.SecretDeleteAccessDenied(Logger, managementCommand.SecretKey, ex.Message);
            return GenericResult.Failure($"Access denied deleting secret '{managementCommand.SecretKey}': {ex.Message}");
        }
    }

    private async Task<IGenericResult> ExecuteListSecrets(ISecretManagerCommand managementCommand, CancellationToken cancellationToken)
    {
        try
        {
            var maxResults = managementCommand.Parameters.TryGetValue("MaxResults", out var maxObj) && 
                            maxObj is int max ? max : Configuration.MaxSecretsPerPage ?? 25;

            var includeDeleted = managementCommand.Parameters.TryGetValue("IncludeDeleted", out var includeDeletedObj) && 
                                includeDeletedObj is bool includeDeletedValue && includeDeletedValue;

            var filter = managementCommand.Parameters.TryGetValue("Filter", out var filterObj) ? 
                        filterObj?.ToString() : null;

            var secretMetadataList = new List<ISecretMetadata>();
            
            await foreach (var secretProperties in _secretClient.GetPropertiesOfSecretsAsync(cancellationToken).ConfigureAwait(false))
            {
                if (secretMetadataList.Count >= maxResults)
                    break;

                // Apply filter if specified
                if (!string.IsNullOrWhiteSpace(filter) && 
                    !secretProperties.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var metadata = CreateSecretMetadata(secretProperties);
                secretMetadataList.Add(metadata);
            }

            AzureKeyVaultServiceLog.SecretsListed(Logger, secretMetadataList.Count);

            return GenericResult<IReadOnlyList<ISecretMetadata>>.Success(secretMetadataList.AsReadOnly());
        }
        catch (RequestFailedException ex) when (ex.Status == 403)
        {
            AzureKeyVaultServiceLog.SecretsListAccessDenied(Logger, ex.Message);
            return GenericResult.Failure($"Access denied listing secrets: {ex.Message}");
        }
    }

    private async Task<IGenericResult> ExecuteGetSecretVersions(ISecretManagerCommand managementCommand, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(managementCommand.SecretKey))
            return GenericResult.Failure("SecretKey is required for GetSecretVersions operation.");

        try
        {
            var secretName = SanitizeSecretName(managementCommand.SecretKey);
            var versionMetadataList = new List<ISecretMetadata>();

            await foreach (var versionProperties in _secretClient.GetPropertiesOfSecretVersionsAsync(secretName, cancellationToken).ConfigureAwait(false))
            {
                var metadata = CreateSecretMetadata(versionProperties);
                versionMetadataList.Add(metadata);
            }

            AzureKeyVaultServiceLog.SecretVersionsRetrieved(Logger, versionMetadataList.Count, secretName);

            return GenericResult<IReadOnlyList<ISecretMetadata>>.Success(versionMetadataList.AsReadOnly());
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            AzureKeyVaultServiceLog.SecretNotFoundForVersionListing(Logger, managementCommand.SecretKey);
            return GenericResult.Failure($"Secret '{managementCommand.SecretKey}' not found.");
        }
    }

    private SecretClient CreateSecretClient()
    {
        var vaultUri = new Uri(Configuration.VaultUri!);
        var credential = CreateCredential();
        
        var options = new SecretClientOptions();
        
        if (Configuration.EnableTracing)
        {
            // Configure tracing if enabled
            options.Diagnostics.IsDistributedTracingEnabled = true;
        }

        return new SecretClient(vaultUri, credential, options);
    }

    private TokenCredential CreateCredential()
    {
        return Configuration.AuthenticationMethod switch
        {
            "ManagedIdentity" => CreateManagedIdentityCredential(),
            "ServicePrincipal" => CreateServicePrincipalCredential(),
            "Certificate" => CreateCertificateCredential(),
            "DeviceCode" => CreateDeviceCodeCredential(),
            _ => throw new InvalidOperationException($"Unsupported authentication method: {Configuration.AuthenticationMethod}")
        };
    }

    private ManagedIdentityCredential CreateManagedIdentityCredential()
    {
        if (!string.IsNullOrWhiteSpace(Configuration.ManagedIdentityId))
        {
            return new ManagedIdentityCredential(Configuration.ManagedIdentityId);
        }

        return new ManagedIdentityCredential();
    }

    private ClientSecretCredential CreateServicePrincipalCredential()
    {
        if (string.IsNullOrWhiteSpace(Configuration.TenantId) ||
            string.IsNullOrWhiteSpace(Configuration.ClientId) ||
            string.IsNullOrWhiteSpace(Configuration.ClientSecret))
        {
            throw new InvalidOperationException("TenantId, ClientId, and ClientSecret are required for ServicePrincipal authentication.");
        }

        return new ClientSecretCredential(Configuration.TenantId, Configuration.ClientId, Configuration.ClientSecret);
    }

    private ClientCertificateCredential CreateCertificateCredential()
    {
        if (string.IsNullOrWhiteSpace(Configuration.TenantId) ||
            string.IsNullOrWhiteSpace(Configuration.ClientId) ||
            string.IsNullOrWhiteSpace(Configuration.CertificatePath))
        {
            throw new InvalidOperationException("TenantId, ClientId, and CertificatePath are required for Certificate authentication.");
        }

        return new ClientCertificateCredential(
            Configuration.TenantId,
            Configuration.ClientId,
            Configuration.CertificatePath,
            new ClientCertificateCredentialOptions
            {
                // Add certificate password if provided
                // Note: In a real implementation, you might want to load the certificate differently
            });
    }

    private static DeviceCodeCredential CreateDeviceCodeCredential()
    {
        return new DeviceCodeCredential();
    }

    private void ValidateConfiguration()
    {
        var validationResult = _configurationValidator.Validate(Configuration);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new InvalidOperationException($"Configuration validation failed: {errors}");
        }
    }

    private static string SanitizeSecretName(string secretKey)
    {
        // Azure Key Vault secret names can only contain alphanumeric characters and hyphens
        // and must be 1-127 characters long
        return secretKey.Replace("_", "-").Replace(" ", "-").ToLowerInvariant();
    }

    private static SecretValue CreateSecretValue(KeyVaultSecret secret, bool includeMetadata)
    {
        var metadata = includeMetadata ? CreateSecretMetadata(secret.Properties) : null;
        
        var metadataDict = includeMetadata 
            ? new Dictionary<string, object>(StringComparer.Ordinal) { ["Metadata"] = metadata! }
            : null;

        return new SecretValue(
            secret.Name,
            secret.Value,
            secret.Properties.Version,
            secret.Properties.CreatedOn,
            secret.Properties.UpdatedOn,
            secret.Properties.ExpiresOn,
            metadataDict);
    }

    private static AzureKeyVaultSecretMetadata CreateSecretMetadata(SecretProperties properties)
    {
        return new AzureKeyVaultSecretMetadata(
            properties.Name,
            properties.Version,
            properties.CreatedOn,
            properties.UpdatedOn,
            properties.ExpiresOn,
            properties.Enabled ?? true,
            properties.Tags as IReadOnlyDictionary<string, string>);
    }
}