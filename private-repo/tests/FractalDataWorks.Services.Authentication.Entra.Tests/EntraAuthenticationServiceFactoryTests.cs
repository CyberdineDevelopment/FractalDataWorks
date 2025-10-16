using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Security;
using FractalDataWorks.Services.Authentication.AzureEntra;

namespace FractalDataWorks.Services.Authentication.Entra.Tests;

public sealed class EntraAuthenticationServiceFactoryTests
{
    [Fact]
    public void Constructor_WithLogger_CreatesInstance()
    {
        // Arrange
        var logger = new Mock<ILogger<GenericServiceFactory<IAuthenticationService, IAuthenticationConfiguration>>>().Object;

        // Act
        var factory = new EntraAuthenticationServiceFactory(logger);

        // Assert
        factory.ShouldNotBeNull();
        factory.ShouldBeAssignableTo<IEntraAuthenticationServiceFactory>();
    }

    [Fact]
    public void Constructor_WithoutLogger_CreatesInstance()
    {
        // Act
        var factory = new EntraAuthenticationServiceFactory();

        // Assert
        factory.ShouldNotBeNull();
        factory.ShouldBeAssignableTo<IEntraAuthenticationServiceFactory>();
    }

    [Fact]
    public void Factory_ImplementsCorrectInterfaces()
    {
        // Arrange
        var factory = new EntraAuthenticationServiceFactory();

        // Assert
        factory.ShouldBeAssignableTo<IEntraAuthenticationServiceFactory>();
        factory.ShouldBeAssignableTo<GenericServiceFactory<IAuthenticationService, IAuthenticationConfiguration>>();
    }
}
