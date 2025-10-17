using FluentValidation;
using FractalDataWorks.Services.Connections.MsSql;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Connections.MsSql.Tests;

public class MsSqlServiceTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MsSqlService>>();
        var mockConfiguration = new Mock<MsSqlConfiguration>();
        var mockValidator = new Mock<IValidator<MsSqlConfiguration>>();

        // Act
        var service = new MsSqlService(mockLogger.Object, mockConfiguration.Object);

        // Assert
        service.ShouldNotBeNull();
    }

    [Fact]
    public void Configuration_ShouldReturnConfigurationInstance()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MsSqlService>>();
        var configuration = new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=TestDb;Integrated Security=true;"
        };

        // Act
        var service = new MsSqlService(mockLogger.Object, configuration);

        // Assert
        service.Configuration.ShouldBe(configuration);
    }
}
