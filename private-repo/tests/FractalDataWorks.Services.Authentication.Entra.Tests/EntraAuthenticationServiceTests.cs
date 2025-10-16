using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Security;
using FractalDataWorks.Services.Authentication.AzureEntra;
using FractalDataWorks.Services.Authentication.AzureEntra.Configuration;

namespace FractalDataWorks.Services.Authentication.Entra.Tests;

public sealed class EntraAuthenticationServiceTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var logger = new Mock<ILogger<EntraAuthenticationService>>().Object;
        var configuration = CreateValidConfiguration();

        // Act
        var service = new EntraAuthenticationService(logger, configuration);

        // Assert
        service.ShouldNotBeNull();
    }

    [Fact]
    public async Task Authenticate_WithNullToken_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.Authenticate(null!, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task Authenticate_WithEmptyToken_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.Authenticate(string.Empty, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task Authenticate_WithWhitespaceToken_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.Authenticate("   ", CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task Authenticate_WithValidToken_ReturnsNotImplementedFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.Authenticate("valid-token", CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateToken_WithNullToken_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.ValidateToken(null!, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateToken_WithEmptyToken_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.ValidateToken(string.Empty, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateToken_WithWhitespaceToken_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.ValidateToken("   ", CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateToken_WithValidToken_ReturnsSuccessWithFalse()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.ValidateToken("valid-token", CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeFalse();
    }

    [Fact]
    public async Task RefreshToken_WithNullToken_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.RefreshToken(null!, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task RefreshToken_WithEmptyToken_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.RefreshToken(string.Empty, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task RefreshToken_WithWhitespaceToken_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.RefreshToken("   ", CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.RefreshToken("valid-refresh-token", CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task RevokeToken_WithNullToken_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.RevokeToken(null!, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task RevokeToken_WithEmptyToken_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.RevokeToken(string.Empty, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task RevokeToken_WithWhitespaceToken_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.RevokeToken("   ", CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task RevokeToken_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.RevokeToken("valid-token", CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_WithCommand_ReturnsCommandNotSupportedFailure()
    {
        // Arrange
        var service = CreateService();
        var command = new Mock<IAuthenticationCommand>().Object;

        // Act - use reflection to call the protected Execute method
        var methods = typeof(EntraAuthenticationService).GetMethods(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var method = methods.First(m =>
            m.Name == "Execute" &&
            !m.IsGenericMethod &&
            m.GetParameters().Length == 1 &&
            m.GetParameters()[0].ParameterType == typeof(IAuthenticationCommand));
        var task = (Task<IGenericResult>)method.Invoke(service, [command])!;
        var result = await task;

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_Generic_WithCommand_ReturnsCommandNotSupportedFailure()
    {
        // Arrange
        var service = CreateService();
        var command = new Mock<IAuthenticationCommand>().Object;

        // Act - use reflection to call the protected Execute<T> method
        var methods = typeof(EntraAuthenticationService).GetMethods(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var method = methods.First(m =>
            m.Name == "Execute" &&
            m.IsGenericMethod &&
            m.GetParameters().Length == 1 &&
            m.GetParameters()[0].ParameterType == typeof(IAuthenticationCommand));
        var genericMethod = method.MakeGenericMethod(typeof(string));
        var task = (Task<IGenericResult<string>>)genericMethod.Invoke(service, [command])!;
        var result = await task;

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_WithCommandAndCancellationToken_ReturnsCommandNotSupportedFailure()
    {
        // Arrange
        var service = CreateService();
        var command = new Mock<IAuthenticationCommand>().Object;

        // Act - use reflection to call the protected Execute method with cancellation token
        var methods = typeof(EntraAuthenticationService).GetMethods(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var method = methods.First(m =>
            m.Name == "Execute" &&
            !m.IsGenericMethod &&
            m.GetParameters().Length == 2 &&
            m.GetParameters()[0].ParameterType == typeof(IAuthenticationCommand) &&
            m.GetParameters()[1].ParameterType == typeof(CancellationToken));
        var task = (Task<IGenericResult>)method.Invoke(service, [command, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_GenericWithCancellationToken_WithCommand_ReturnsCommandNotSupportedFailure()
    {
        // Arrange
        var service = CreateService();
        var command = new Mock<IAuthenticationCommand>().Object;

        // Act - use reflection to call the protected Execute<TOut> method with cancellation token
        var methods = typeof(EntraAuthenticationService).GetMethods(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var method = methods.First(m =>
            m.Name == "Execute" &&
            m.IsGenericMethod &&
            m.GetParameters().Length == 2 &&
            m.GetParameters()[0].ParameterType == typeof(IAuthenticationCommand) &&
            m.GetParameters()[1].ParameterType == typeof(CancellationToken));
        var genericMethod = method.MakeGenericMethod(typeof(string));
        var task = (Task<IGenericResult<string>>)genericMethod.Invoke(service, [command, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeTrue();
    }

    private static EntraAuthenticationService CreateService()
    {
        var logger = new Mock<ILogger<EntraAuthenticationService>>().Object;
        var configuration = CreateValidConfiguration();
        return new EntraAuthenticationService(logger, configuration);
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
}
