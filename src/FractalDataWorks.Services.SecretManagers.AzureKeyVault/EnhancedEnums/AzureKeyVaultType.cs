using System;
using System.Collections.Generic;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.SecretManagers.Abstractions;
using FractalDataWorks.Services.SecretManager;

namespace FractalDataWorks.Services.SecretManagers.AzureKeyVault.EnhancedEnums;

/// <summary>
/// Enhanced enum type for Azure Key Vault secret management service.
/// </summary>
[EnumOption]
public sealed class AzureKeyVaultType : SecretManagerServiceType<AzureKeyVaultType, ISecretService, ISecretManagerConfiguration, IServiceFactory<ISecretService, ISecretManagerConfiguration>>,IEnumOption<AzureKeyVaultType>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultType"/> class.
    /// </summary>
    public AzureKeyVaultType() : base(1, nameof(AzureKeyVault), "Microsoft Azure Key Vault secret management service")
    {
    }

    /// <inheritdoc/>
    public override string[] SupportedSecretStores =>
    [
        nameof(AzureKeyVault), 
        "Azure Key Vault", 
        "KeyVault", 
        "Microsoft Azure Key Vault"
    ];

    /// <inheritdoc/>
    public override string ProviderName => "Azure.Security.KeyVault.Secrets";

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedAuthenticationMethods =>
    [
        "ManagedIdentity",
        "ServicePrincipal",
        "Certificate",
        "DeviceCode"
    ];

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedOperations =>
    [
        "GetSecret",
        "SetSecret",
        "DeleteSecret",
        "ListSecrets",
        "GetSecretVersions",
        "RestoreSecret",
        "BackupSecret",
        "PurgeSecret"
    ];

    /// <inheritdoc/>
    public override int Priority => 100;

    /// <inheritdoc/>
    public override bool SupportsEncryptionAtRest => true;

    /// <inheritdoc/>
    public override bool SupportsAuditLogging => true;


}