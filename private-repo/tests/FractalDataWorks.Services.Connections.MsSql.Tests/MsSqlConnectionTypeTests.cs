using FractalDataWorks.Services.Connections.MsSql;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Connections.MsSql.Tests;

public class MsSqlConnectionTypeTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Act
        var connectionType = new MsSqlConnectionType();

        // Assert
        connectionType.ShouldNotBeNull();
    }

    [Fact]
    public void Id_ShouldHaveValidValue()
    {
        // Arrange & Act
        var connectionType = new MsSqlConnectionType();

        // Assert
        connectionType.Id.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Name_ShouldBeCorrect()
    {
        // Arrange & Act
        var connectionType = new MsSqlConnectionType();

        // Assert
        connectionType.Name.ShouldNotBeNullOrEmpty();
        connectionType.Name.ShouldBe("MsSql");
    }

    [Fact]
    public void DisplayName_ShouldNotBeNullOrEmpty()
    {
        // Arrange & Act
        var connectionType = new MsSqlConnectionType();

        // Assert
        connectionType.DisplayName.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Description_ShouldNotBeNullOrEmpty()
    {
        // Arrange & Act
        var connectionType = new MsSqlConnectionType();

        // Assert
        connectionType.Description.ShouldNotBeNullOrEmpty();
    }
}
