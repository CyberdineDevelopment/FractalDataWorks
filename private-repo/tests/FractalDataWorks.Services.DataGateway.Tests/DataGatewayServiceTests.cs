using FractalDataWorks.Data.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Data.Abstractions;
using FractalDataWorks.Services.DataGateway;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace FractalDataWorks.Services.DataGateway.Tests;

public sealed class DataGatewayServiceTests
{
    private readonly Mock<ILogger<DataGatewayService>> _loggerMock;
    private readonly Mock<IDataConnectionProvider> _connectionProviderMock;

    public DataGatewayServiceTests()
    {
        _loggerMock = new Mock<ILogger<DataGatewayService>>();
        _connectionProviderMock = new Mock<IDataConnectionProvider>();
    }

    [Fact]
    public void Constructor_ShouldInitializeService()
    {
        // Act
        var service = new DataGatewayService(_loggerMock.Object, _connectionProviderMock.Object);

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeAssignableTo<IDataGateway>();
    }

    [Fact]
    public async Task Execute_ShouldReturnFailure_WhenConnectionNotFound()
    {
        // Arrange
        var service = new DataGatewayService(_loggerMock.Object, _connectionProviderMock.Object);
        var commandMock = new Mock<IDataCommand>();
        commandMock.Setup(c => c.ConnectionName).Returns("test-connection");
        commandMock.Setup(c => c.CommandType).Returns("Query");

        _connectionProviderMock
            .Setup(p => p.GetConnection("test-connection"))
            .ReturnsAsync(GenericResult<IDataConnection>.Failure("Connection not found"));

        // Act
        var result = await service.Execute<string>(commandMock.Object,TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Messages.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Execute_ShouldReturnFailure_WhenConnectionResultValueIsNull()
    {
        // Arrange
        var service = new DataGatewayService(_loggerMock.Object, _connectionProviderMock.Object);
        var commandMock = new Mock<IDataCommand>();
        commandMock.Setup(c => c.ConnectionName).Returns("test-connection");
        commandMock.Setup(c => c.CommandType).Returns("Query");

        _connectionProviderMock
            .Setup(p => p.GetConnection("test-connection"))
            .ReturnsAsync(GenericResult<IDataConnection>.Success(null!));

        // Act
        var result = await service.Execute<string>(commandMock.Object,TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task Execute_ShouldCallConnectionExecute_WhenConnectionFound()
    {
        // Arrange
        var service = new DataGatewayService(_loggerMock.Object, _connectionProviderMock.Object);
        var commandMock = new Mock<IDataCommand>();
        commandMock.Setup(c => c.ConnectionName).Returns("test-connection");
        commandMock.Setup(c => c.CommandType).Returns("Query");

        var connectionMock = new Mock<IDataConnection>();
        var expectedResult = GenericResult<string>.Success("test-result");
        connectionMock
            .Setup(c => c.Execute<string>(It.IsAny<IDataCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        _connectionProviderMock
            .Setup(p => p.GetConnection("test-connection"))
            .ReturnsAsync(GenericResult<IDataConnection>.Success(connectionMock.Object));

        // Act
        var result = await service.Execute<string>(commandMock.Object,TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("test-result");
        connectionMock.Verify(c => c.Execute<string>(commandMock.Object, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldPassCancellationToken_ToConnection()
    {
        // Arrange
        var service = new DataGatewayService(_loggerMock.Object, _connectionProviderMock.Object);
        var commandMock = new Mock<IDataCommand>();
        commandMock.Setup(c => c.ConnectionName).Returns("test-connection");
        commandMock.Setup(c => c.CommandType).Returns("Query");

        var connectionMock = new Mock<IDataConnection>();
        var cancellationToken = new CancellationToken();
        var expectedResult = GenericResult<int>.Success(42);
        connectionMock
            .Setup(c => c.Execute<int>(It.IsAny<IDataCommand>(), cancellationToken))
            .ReturnsAsync(expectedResult);

        _connectionProviderMock
            .Setup(p => p.GetConnection("test-connection"))
            .ReturnsAsync(GenericResult<IDataConnection>.Success(connectionMock.Object));

        // Act
        var result = await service.Execute<int>(commandMock.Object, cancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
        connectionMock.Verify(c => c.Execute<int>(commandMock.Object, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldLogDebug_WhenRoutingCommand()
    {
        // Arrange
        var service = new DataGatewayService(_loggerMock.Object, _connectionProviderMock.Object);
        var commandMock = new Mock<IDataCommand>();
        commandMock.Setup(c => c.ConnectionName).Returns("test-connection");
        commandMock.Setup(c => c.CommandType).Returns("Query");

        var connectionMock = new Mock<IDataConnection>();
        connectionMock
            .Setup(c => c.Execute<string>(It.IsAny<IDataCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GenericResult<string>.Success("result"));

        _connectionProviderMock
            .Setup(p => p.GetConnection("test-connection"))
            .ReturnsAsync(GenericResult<IDataConnection>.Success(connectionMock.Object));

        // Act
        await service.Execute<string>(commandMock.Object,TestContext.Current.CancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldLogError_WhenConnectionNotFound()
    {
        // Arrange
        var service = new DataGatewayService(_loggerMock.Object, _connectionProviderMock.Object);
        var commandMock = new Mock<IDataCommand>();
        commandMock.Setup(c => c.ConnectionName).Returns("missing-connection");
        commandMock.Setup(c => c.CommandType).Returns("Query");

        _connectionProviderMock
            .Setup(p => p.GetConnection("missing-connection"))
            .ReturnsAsync(GenericResult<IDataConnection>.Failure("Not found"));

        // Act
        await service.Execute<string>(commandMock.Object,TestContext.Current.CancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
