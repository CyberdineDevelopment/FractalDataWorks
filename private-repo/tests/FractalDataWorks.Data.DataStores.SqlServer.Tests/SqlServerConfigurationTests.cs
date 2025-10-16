using FractalDataWorks.Data.DataStores.SqlServer;
using Shouldly;

namespace FractalDataWorks.Data.DataStores.SqlServer.Tests;

public sealed class SqlServerConfigurationTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var config = new SqlServerConfiguration();

        // Assert
        config.ConnectionString.ShouldBe(string.Empty);
        config.CommandTimeout.ShouldBe(30);
        config.EnableConnectionPooling.ShouldBeTrue();
        config.MaxPoolSize.ShouldBe(100);
        config.UseAzureAuthentication.ShouldBeFalse();
    }

    [Fact]
    public void ConnectionString_ShouldBeSettable()
    {
        // Arrange
        var config = new SqlServerConfiguration();
        var connectionString = "Server=localhost;Database=test;";

        // Act
        config.ConnectionString = connectionString;

        // Assert
        config.ConnectionString.ShouldBe(connectionString);
    }

    [Fact]
    public void CommandTimeout_ShouldBeSettable()
    {
        // Arrange
        var config = new SqlServerConfiguration();
        var timeout = 60;

        // Act
        config.CommandTimeout = timeout;

        // Assert
        config.CommandTimeout.ShouldBe(timeout);
    }

    [Fact]
    public void EnableConnectionPooling_ShouldBeSettable()
    {
        // Arrange
        var config = new SqlServerConfiguration();

        // Act
        config.EnableConnectionPooling = false;

        // Assert
        config.EnableConnectionPooling.ShouldBeFalse();
    }

    [Fact]
    public void MaxPoolSize_ShouldBeSettable()
    {
        // Arrange
        var config = new SqlServerConfiguration();
        var poolSize = 50;

        // Act
        config.MaxPoolSize = poolSize;

        // Assert
        config.MaxPoolSize.ShouldBe(poolSize);
    }

    [Fact]
    public void UseAzureAuthentication_ShouldBeSettable()
    {
        // Arrange
        var config = new SqlServerConfiguration();

        // Act
        config.UseAzureAuthentication = true;

        // Assert
        config.UseAzureAuthentication.ShouldBeTrue();
    }

    [Fact]
    public void AllProperties_ShouldBeIndependent()
    {
        // Arrange
        var config = new SqlServerConfiguration
        {
            ConnectionString = "Server=test;",
            CommandTimeout = 45,
            EnableConnectionPooling = false,
            MaxPoolSize = 75,
            UseAzureAuthentication = true
        };

        // Assert
        config.ConnectionString.ShouldBe("Server=test;");
        config.CommandTimeout.ShouldBe(45);
        config.EnableConnectionPooling.ShouldBeFalse();
        config.MaxPoolSize.ShouldBe(75);
        config.UseAzureAuthentication.ShouldBeTrue();
    }
}
