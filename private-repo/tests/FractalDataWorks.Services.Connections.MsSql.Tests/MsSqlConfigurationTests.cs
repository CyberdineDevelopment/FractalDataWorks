using FractalDataWorks.Services.Connections.MsSql;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Connections.MsSql.Tests;

public class MsSqlConfigurationTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Act
        var configuration = new MsSqlConfiguration();

        // Assert
        configuration.ShouldNotBeNull();
    }

    [Fact]
    public void ConnectionString_ShouldGetAndSet()
    {
        // Arrange
        var configuration = new MsSqlConfiguration();
        var connectionString = "Server=localhost;Database=TestDb;Integrated Security=true;";

        // Act
        configuration.ConnectionString = connectionString;

        // Assert
        configuration.ConnectionString.ShouldBe(connectionString);
    }

    [Fact]
    public void Name_ShouldDefaultToCorrectValue()
    {
        // Arrange & Act
        var configuration = new MsSqlConfiguration();

        // Assert
        configuration.Name.ShouldNotBeNullOrEmpty();
    }
}
