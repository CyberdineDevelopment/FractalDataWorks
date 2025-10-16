using FractalDataWorks.Services.Authentication.AzureEntra.Configuration;
using Shouldly;
using System;using Xunit;
namespace FractalDataWorks.Services.Authentication.Entra.Tests.Configuration;

public sealed class AzureEntraConfigurationTests
{
    [Fact]
    public void SectionName_ReturnsCorrectValue()
    {
        // Arrange
        var configuration = new AzureEntraConfiguration();

        // Act
        var sectionName = configuration.SectionName;

        // Assert
        sectionName.ShouldBe("AzureEntra");
    }

    [Fact]
    public void AuthenticationType_ReturnsCorrectValue()
    {
        // Arrange
        var configuration = new AzureEntraConfiguration();

        // Act
        var authenticationType = configuration.AuthenticationType;

        // Assert
        authenticationType.ShouldBe("AzureEntra");
    }

    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        // Act
        var configuration = new AzureEntraConfiguration();

        // Assert
        configuration.ClientId.ShouldBe(string.Empty);
        configuration.ClientSecret.ShouldBe(string.Empty);
        configuration.TenantId.ShouldBe(string.Empty);
        configuration.Authority.ShouldBe(string.Empty);
        configuration.RedirectUri.ShouldBe(string.Empty);
        configuration.Scopes.ShouldBeEmpty();
        configuration.EnableTokenCaching.ShouldBeTrue();
        configuration.TokenCacheLifetimeMinutes.ShouldBe(60);
        configuration.Instance.ShouldBe("https://login.microsoftonline.com");
        configuration.ClientType.ShouldBe("Confidential");
        configuration.ValidateIssuer.ShouldBeTrue();
        configuration.ValidateAudience.ShouldBeTrue();
        configuration.ValidateLifetime.ShouldBeTrue();
        configuration.ValidateIssuerSigningKey.ShouldBeTrue();
        configuration.ClockSkewToleranceMinutes.ShouldBe(5);
        configuration.CacheFilePath.ShouldBeNull();
        configuration.EnablePiiLogging.ShouldBeFalse();
        configuration.HttpTimeoutSeconds.ShouldBe(30);
        configuration.MaxRetryAttempts.ShouldBe(3);
        configuration.AdditionalValidAudiences.ShouldBeEmpty();
        configuration.AdditionalValidIssuers.ShouldBeEmpty();
    }

    [Fact]
    public void InitProperties_CanBeSet()
    {
        // Arrange & Act
        var configuration = new AzureEntraConfiguration
        {
            ClientId = "test-client-id",
            ClientSecret = "test-secret",
            TenantId = "test-tenant",
            Authority = "https://login.microsoftonline.com/test",
            RedirectUri = "https://localhost:5001/callback",
            Scopes = ["scope1", "scope2"],
            EnableTokenCaching = false,
            TokenCacheLifetimeMinutes = 120,
            Instance = "https://login.microsoftonline.us",
            ClientType = "Public",
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false,
            ClockSkewToleranceMinutes = 10,
            CacheFilePath = "/path/to/cache",
            EnablePiiLogging = true,
            HttpTimeoutSeconds = 60,
            MaxRetryAttempts = 5,
            AdditionalValidAudiences = ["audience1"],
            AdditionalValidIssuers = ["issuer1"]
        };

        // Assert
        configuration.ClientId.ShouldBe("test-client-id");
        configuration.ClientSecret.ShouldBe("test-secret");
        configuration.TenantId.ShouldBe("test-tenant");
        configuration.Authority.ShouldBe("https://login.microsoftonline.com/test");
        configuration.RedirectUri.ShouldBe("https://localhost:5001/callback");
        configuration.Scopes.ShouldBe(["scope1", "scope2"]);
        configuration.EnableTokenCaching.ShouldBeFalse();
        configuration.TokenCacheLifetimeMinutes.ShouldBe(120);
        configuration.Instance.ShouldBe("https://login.microsoftonline.us");
        configuration.ClientType.ShouldBe("Public");
        configuration.ValidateIssuer.ShouldBeFalse();
        configuration.ValidateAudience.ShouldBeFalse();
        configuration.ValidateLifetime.ShouldBeFalse();
        configuration.ValidateIssuerSigningKey.ShouldBeFalse();
        configuration.ClockSkewToleranceMinutes.ShouldBe(10);
        configuration.CacheFilePath.ShouldBe("/path/to/cache");
        configuration.EnablePiiLogging.ShouldBeTrue();
        configuration.HttpTimeoutSeconds.ShouldBe(60);
        configuration.MaxRetryAttempts.ShouldBe(5);
        configuration.AdditionalValidAudiences.ShouldBe(["audience1"]);
        configuration.AdditionalValidIssuers.ShouldBe(["issuer1"]);
    }

    [Fact]
    public void GetValidator_ReturnsAzureEntraConfigurationValidator()
    {
        // Arrange
        var configuration = new AzureEntraConfiguration();

        // Act
        var validator = configuration.GetType()
            .GetMethod("GetValidator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(configuration, null);

        // Assert
        validator.ShouldNotBeNull();
        validator.ShouldBeOfType<AzureEntraConfigurationValidator>();
    }
}
