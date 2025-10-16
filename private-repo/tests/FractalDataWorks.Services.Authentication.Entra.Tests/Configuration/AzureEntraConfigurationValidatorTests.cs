using FractalDataWorks.Services.Authentication.AzureEntra.Configuration;
using Shouldly;
using System;using Xunit;
namespace FractalDataWorks.Services.Authentication.Entra.Tests.Configuration;

public sealed class AzureEntraConfigurationValidatorTests
{
    [Fact]
    public void Validate_WithValidConfiguration_ReturnsValid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateValidConfiguration();

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithEmptyClientId_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(clientId: string.Empty);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "ClientId");
    }

    [Fact]
    public void Validate_WithInvalidClientIdGuid_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(clientId: "not-a-guid");

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "ClientId");
    }

    [Fact]
    public void Validate_WithEmptyTenantId_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(tenantId: string.Empty);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "TenantId");
    }

    [Fact]
    public void Validate_WithCommonTenantId_ReturnsValid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(tenantId: "common");

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithOrganizationsTenantId_ReturnsValid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(tenantId: "organizations");

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithConsumersTenantId_ReturnsValid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(tenantId: "consumers");

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithEmptyInstance_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(instance: string.Empty);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Instance");
    }

    [Fact]
    public void Validate_WithInvalidInstanceUri_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(instance: "not-a-uri");

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Instance");
    }

    [Fact]
    public void Validate_WithEmptyRedirectUri_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(redirectUri: string.Empty);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "RedirectUri");
    }

    [Fact]
    public void Validate_WithInvalidRedirectUri_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(redirectUri: "not-a-uri");

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "RedirectUri");
    }

    [Fact]
    public void Validate_WithEmptyAuthority_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(authority: string.Empty);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Authority");
    }

    [Fact]
    public void Validate_WithInvalidAuthority_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(authority: "not-a-uri");

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Authority");
    }

    [Fact]
    public void Validate_WithEmptyScopes_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(scopes: []);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Scopes");
    }

    [Fact]
    public void Validate_WithNullScopes_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = new AzureEntraConfiguration
        {
            ClientId = Guid.NewGuid().ToString(),
            ClientSecret = "test-secret",
            TenantId = Guid.NewGuid().ToString(),
            Authority = "https://login.microsoftonline.com/test",
            RedirectUri = "https://localhost:5001/callback",
            Scopes = null!,
            Instance = "https://login.microsoftonline.com",
            ClientType = "Confidential"
        };

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Scopes");
    }

    [Fact]
    public void Validate_WithWhitespaceScope_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(scopes: ["   "]);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Scopes");
    }

    [Fact]
    public void Validate_WithInvalidClientType_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(clientType: "Invalid");

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "ClientType");
    }

    [Fact]
    public void Validate_WithPublicClientType_ReturnsValid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(clientType: "Public", clientSecret: string.Empty);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithConfidentialClientTypeAndNoSecret_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(clientType: "Confidential", clientSecret: string.Empty);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "ClientSecret");
    }

    [Fact]
    public void Validate_WithTokenCacheLifetimeZero_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(tokenCacheLifetimeMinutes: 0);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "TokenCacheLifetimeMinutes");
    }

    [Fact]
    public void Validate_WithTokenCacheLifetimeTooLarge_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(tokenCacheLifetimeMinutes: 1441);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "TokenCacheLifetimeMinutes");
    }

    [Fact]
    public void Validate_WithClockSkewNegative_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(clockSkewToleranceMinutes: -1);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "ClockSkewToleranceMinutes");
    }

    [Fact]
    public void Validate_WithClockSkewTooLarge_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(clockSkewToleranceMinutes: 31);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "ClockSkewToleranceMinutes");
    }

    [Fact]
    public void Validate_WithHttpTimeoutZero_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(httpTimeoutSeconds: 0);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "HttpTimeoutSeconds");
    }

    [Fact]
    public void Validate_WithHttpTimeoutTooLarge_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(httpTimeoutSeconds: 301);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "HttpTimeoutSeconds");
    }

    [Fact]
    public void Validate_WithMaxRetryNegative_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(maxRetryAttempts: -1);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "MaxRetryAttempts");
    }

    [Fact]
    public void Validate_WithMaxRetryTooLarge_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(maxRetryAttempts: 11);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "MaxRetryAttempts");
    }

    [Fact]
    public void Validate_WithValidCacheFilePath_ReturnsValid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(cacheFilePath: "C:\\path\\to\\cache.dat");

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithNullCacheFilePath_ReturnsValid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(cacheFilePath: null);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithInvalidCacheFilePath_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(cacheFilePath: "relative/path");

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "CacheFilePath");
    }

    [Fact]
    public void Validate_WithInvalidAdditionalAudience_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(additionalValidAudiences: ["not-a-uri"]);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "AdditionalValidAudiences[0]");
    }

    [Fact]
    public void Validate_WithValidAdditionalAudience_ReturnsValid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(additionalValidAudiences: ["https://example.com/api"]);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithInvalidAdditionalIssuer_ReturnsInvalid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(additionalValidIssuers: ["not-a-uri"]);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "AdditionalValidIssuers[0]");
    }

    [Fact]
    public void Validate_WithValidAdditionalIssuer_ReturnsValid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(additionalValidIssuers: ["https://example.com"]);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithEmptyCacheFilePath_ReturnsValid()
    {
        // Arrange
        var validator = new AzureEntraConfigurationValidator();
        var configuration = CreateConfiguration(cacheFilePath: string.Empty);

        // Act
        var result = validator.Validate(configuration);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    private static AzureEntraConfiguration CreateValidConfiguration()
    {
        return new AzureEntraConfiguration
        {
            ClientId = Guid.NewGuid().ToString(),
            ClientSecret = "test-secret",
            TenantId = Guid.NewGuid().ToString(),
            Authority = "https://login.microsoftonline.com/test",
            RedirectUri = "https://localhost:5001/callback",
            Scopes = ["api://test/.default"],
            Instance = "https://login.microsoftonline.com",
            ClientType = "Confidential"
        };
    }

    private static AzureEntraConfiguration CreateConfiguration(
        string? clientId = null,
        string? clientSecret = null,
        string? tenantId = null,
        string? authority = null,
        string? redirectUri = null,
        string[]? scopes = null,
        string? instance = null,
        string? clientType = null,
        int? tokenCacheLifetimeMinutes = null,
        int? clockSkewToleranceMinutes = null,
        int? httpTimeoutSeconds = null,
        int? maxRetryAttempts = null,
        string? cacheFilePath = null,
        string[]? additionalValidAudiences = null,
        string[]? additionalValidIssuers = null)
    {
        var baseConfig = CreateValidConfiguration();
        return new AzureEntraConfiguration
        {
            ClientId = clientId ?? baseConfig.ClientId,
            ClientSecret = clientSecret ?? baseConfig.ClientSecret,
            TenantId = tenantId ?? baseConfig.TenantId,
            Authority = authority ?? baseConfig.Authority,
            RedirectUri = redirectUri ?? baseConfig.RedirectUri,
            Scopes = scopes ?? baseConfig.Scopes,
            Instance = instance ?? baseConfig.Instance,
            ClientType = clientType ?? baseConfig.ClientType,
            TokenCacheLifetimeMinutes = tokenCacheLifetimeMinutes ?? baseConfig.TokenCacheLifetimeMinutes,
            ClockSkewToleranceMinutes = clockSkewToleranceMinutes ?? baseConfig.ClockSkewToleranceMinutes,
            HttpTimeoutSeconds = httpTimeoutSeconds ?? baseConfig.HttpTimeoutSeconds,
            MaxRetryAttempts = maxRetryAttempts ?? baseConfig.MaxRetryAttempts,
            CacheFilePath = cacheFilePath ?? baseConfig.CacheFilePath,
            AdditionalValidAudiences = additionalValidAudiences ?? baseConfig.AdditionalValidAudiences,
            AdditionalValidIssuers = additionalValidIssuers ?? baseConfig.AdditionalValidIssuers
        };
    }
}
