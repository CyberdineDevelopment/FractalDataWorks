using FractalDataWorks.Services.Authentication.AzureEntra;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using System;using System.Collections.Generic;using Xunit;
namespace FractalDataWorks.Services.Authentication.Entra.Tests;

public sealed class EntraAuthenticationServiceTypeTests
{
    [Fact]
    public void Constructor_CreatesInstanceWithCorrectValues()
    {
        // Act
        var serviceType = new EntraAuthenticationServiceType();

        // Assert
        serviceType.ShouldNotBeNull();
        serviceType.Id.ShouldBe(1);
        serviceType.Name.ShouldBe("AzureEntra");
        serviceType.ProviderName.ShouldBe("Microsoft.Identity.Client");
        serviceType.SupportsMultiTenant.ShouldBeTrue();
        serviceType.SupportsTokenCaching.ShouldBeTrue();
        serviceType.SupportsTokenRefresh.ShouldBeTrue();
        serviceType.Category.ShouldBe("Authentication");
    }

    [Fact]
    public void Priority_ReturnsExpectedValue()
    {
        // Arrange
        var serviceType = new EntraAuthenticationServiceType();

        // Act
        var priority = serviceType.Priority;

        // Assert
        priority.ShouldBe(90);
    }

    [Fact]
    public void Register_RegistersRequiredServices()
    {
        // Arrange
        var serviceType = new EntraAuthenticationServiceType();
        var services = new ServiceCollection();

        // Act
        serviceType.Register(services);

        // Assert
        services.ShouldNotBeEmpty();
        services.Count.ShouldBe(2);
    }

    [Fact]
    public void Configure_WithValidConfiguration_DoesNotThrow()
    {
        // Arrange
        var serviceType = new EntraAuthenticationServiceType();
        var configuration = new Mock<IConfiguration>().Object;

        // Act & Assert
        Should.NotThrow(() => serviceType.Configure(configuration));
    }

    [Fact]
    public void Configure_WithNullConfiguration_DoesNotThrow()
    {
        // Arrange
        var serviceType = new EntraAuthenticationServiceType();

        // Act & Assert
        Should.NotThrow(() => serviceType.Configure(null!));
    }

    [Fact]
    public void SupportedProtocols_ContainsExpectedProtocols()
    {
        // Arrange
        var serviceType = new EntraAuthenticationServiceType();

        // Act
        var protocols = serviceType.SupportedProtocols;

        // Assert
        protocols.ShouldNotBeNull();
        protocols.Count.ShouldBe(3);
    }

    [Fact]
    public void SupportedFlows_ContainsExpectedFlows()
    {
        // Arrange
        var serviceType = new EntraAuthenticationServiceType();

        // Act
        var flows = serviceType.SupportedFlows;

        // Assert
        flows.ShouldNotBeNull();
        flows.Count.ShouldBe(3);
    }

    [Fact]
    public void SupportedTokenTypes_ContainsExpectedTypes()
    {
        // Arrange
        var serviceType = new EntraAuthenticationServiceType();

        // Act
        var tokenTypes = serviceType.SupportedTokenTypes;

        // Assert
        tokenTypes.ShouldNotBeNull();
        tokenTypes.Count.ShouldBe(3);
    }
}
