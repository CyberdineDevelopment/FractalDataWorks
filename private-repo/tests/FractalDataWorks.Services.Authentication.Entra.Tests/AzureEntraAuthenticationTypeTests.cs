using FractalDataWorks.Services.Authentication.AzureEntra;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using System;using System.Collections.Generic;using Xunit;
namespace FractalDataWorks.Services.Authentication.Entra.Tests;

public sealed class AzureEntraAuthenticationTypeTests
{
    [Fact]
    public void Constructor_CreatesInstanceWithCorrectValues()
    {
        // Act
        var authenticationType = new AzureEntraAuthenticationType();

        // Assert
        authenticationType.ShouldNotBeNull();
        authenticationType.Id.ShouldBe(1);
        authenticationType.Name.ShouldBe("AzureEntra");
        authenticationType.ProviderName.ShouldBe("Microsoft Entra ID");
        authenticationType.SupportsMultiTenant.ShouldBeTrue();
        authenticationType.SupportsTokenCaching.ShouldBeTrue();
        authenticationType.SupportsTokenRefresh.ShouldBeTrue();
        authenticationType.Category.ShouldBe("Authentication");
    }

    [Fact]
    public void Priority_ReturnsExpectedValue()
    {
        // Arrange
        var authenticationType = new AzureEntraAuthenticationType();

        // Act
        var priority = authenticationType.Priority;

        // Assert
        priority.ShouldBe(90);
    }

    [Fact]
    public void Configure_WithValidConfiguration_DoesNotThrow()
    {
        // Arrange
        var authenticationType = new AzureEntraAuthenticationType();
        var configuration = new Mock<IConfiguration>().Object;

        // Act & Assert
        Should.NotThrow(() => authenticationType.Configure(configuration));
    }

    [Fact]
    public void Configure_WithNullConfiguration_DoesNotThrow()
    {
        // Arrange
        var authenticationType = new AzureEntraAuthenticationType();

        // Act & Assert
        Should.NotThrow(() => authenticationType.Configure(null!));
    }

    [Fact]
    public void Register_RegistersRequiredServices()
    {
        // Arrange
        var authenticationType = new AzureEntraAuthenticationType();
        var services = new ServiceCollection();

        // Act
        authenticationType.Register(services);

        // Assert
        services.ShouldNotBeEmpty();
        services.Count.ShouldBe(2);
    }

    [Fact]
    public void SupportedProtocols_ContainsExpectedProtocols()
    {
        // Arrange
        var authenticationType = new AzureEntraAuthenticationType();

        // Act
        var protocols = authenticationType.SupportedProtocols;

        // Assert
        protocols.ShouldNotBeNull();
        protocols.Count.ShouldBe(3);
    }

    [Fact]
    public void SupportedFlows_ContainsExpectedFlows()
    {
        // Arrange
        var authenticationType = new AzureEntraAuthenticationType();

        // Act
        var flows = authenticationType.SupportedFlows;

        // Assert
        flows.ShouldNotBeNull();
        flows.Count.ShouldBe(3);
    }

    [Fact]
    public void SupportedTokenTypes_ContainsExpectedTypes()
    {
        // Arrange
        var authenticationType = new AzureEntraAuthenticationType();

        // Act
        var tokenTypes = authenticationType.SupportedTokenTypes;

        // Assert
        tokenTypes.ShouldNotBeNull();
        tokenTypes.Count.ShouldBe(3);
    }
}
