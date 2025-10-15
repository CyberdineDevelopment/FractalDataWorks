using FractalDataWorks.Data.DataStores.SqlServer;
using FractalDataWorks.Data.DataStores.Abstractions;
using FractalDataWorks.Messages;
using Shouldly;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace FractalDataWorks.Data.DataStores.SqlServer.Tests;

public sealed class SqlServerDataStoreTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var id = "test-id";
        var name = "test-name";
        var config = new SqlServerConfiguration
        {
            ConnectionString = "Server=localhost;Database=test;"
        };

        // Act
        var dataStore = new SqlServerDataStore(id, name, config);

        // Assert
        dataStore.Id.ShouldBe(id);
        dataStore.Name.ShouldBe(name);
        dataStore.StoreType.ShouldBe("SqlServer");
        dataStore.Location.ShouldBe(config.ConnectionString);
        dataStore.Configuration.ShouldBe(config);
        dataStore.Metadata.ShouldNotBeNull();
        dataStore.Metadata.ShouldBeEmpty();
    }

    [Fact]
    public void AvailablePaths_ShouldThrowNotSupportedException()
    {
        // Arrange
        var dataStore = new SqlServerDataStore("id", "name", new SqlServerConfiguration());

        // Act & Assert
        Should.Throw<NotSupportedException>(() => dataStore.AvailablePaths);
    }

    [Fact]
    public void GetPath_ShouldThrowNotSupportedException()
    {
        // Arrange
        var dataStore = new SqlServerDataStore("id", "name", new SqlServerConfiguration());

        // Act & Assert
        Should.Throw<NotSupportedException>(() => dataStore.GetPath("test"));
    }

    [Fact]
    public async Task TestConnection_ShouldReturnFailure_WhenConnectionStringIsInvalid()
    {
        // Arrange
        var config = new SqlServerConfiguration
        {
            ConnectionString = "Invalid Connection String"
        };
        var dataStore = new SqlServerDataStore("id", "name", config);

        // Act
        var result = await dataStore.TestConnection();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Messages.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task TestConnection_ShouldReturnFailure_WhenServerNotReachable()
    {
        // Arrange
        var config = new SqlServerConfiguration
        {
            ConnectionString = "Server=nonexistent-server-12345.database.windows.net;Database=test;Connection Timeout=1;"
        };
        var dataStore = new SqlServerDataStore("id", "name", config);

        // Act
        var result = await dataStore.TestConnection();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Messages.ShouldNotBeEmpty();
        result.Messages.ShouldContain(m => m is ErrorMessage);
    }

    [Fact]
    public async Task DiscoverPaths_ShouldReturnFailure_WhenConnectionStringIsInvalid()
    {
        // Arrange
        var config = new SqlServerConfiguration
        {
            ConnectionString = "Invalid Connection String"
        };
        var dataStore = new SqlServerDataStore("id", "name", config);

        // Act
        var result = await dataStore.DiscoverPaths();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Messages.ShouldNotBeEmpty();
        result.Messages.ShouldContain(m => m is ErrorMessage);
    }

    [Fact]
    public async Task DiscoverPaths_ShouldReturnFailure_WhenServerNotReachable()
    {
        // Arrange
        var config = new SqlServerConfiguration
        {
            ConnectionString = "Server=nonexistent-server-12345.database.windows.net;Database=test;Connection Timeout=1;"
        };
        var dataStore = new SqlServerDataStore("id", "name", config);

        // Act
        var result = await dataStore.DiscoverPaths();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Messages.ShouldNotBeEmpty();
    }

    [Theory]
    [InlineData("MsSql")]
    [InlineData("SqlServer")]
    [InlineData("mssql")]
    [InlineData("sqlserver")]
    [InlineData("MSSQL")]
    [InlineData("SQLSERVER")]
    public void ValidateConnectionCompatibility_ShouldReturnSuccess_ForCompatibleTypes(string connectionType)
    {
        // Arrange
        var dataStore = new SqlServerDataStore("id", "name", new SqlServerConfiguration());

        // Act
        var result = dataStore.ValidateConnectionCompatibility(connectionType);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Messages.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("PostgreSQL")]
    [InlineData("MySQL")]
    [InlineData("Oracle")]
    [InlineData("MongoDB")]
    [InlineData("Rest")]
    public void ValidateConnectionCompatibility_ShouldReturnFailure_ForIncompatibleTypes(string connectionType)
    {
        // Arrange
        var dataStore = new SqlServerDataStore("id", "name", new SqlServerConfiguration());

        // Act
        var result = dataStore.ValidateConnectionCompatibility(connectionType);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Messages.ShouldNotBeEmpty();
        result.Messages.ShouldContain(m => m is ValidationMessage);
    }

    [Fact]
    public async Task UpdateConfiguration_ShouldUpdateConfiguration()
    {
        // Arrange
        var originalConfig = new SqlServerConfiguration
        {
            ConnectionString = "Server=original;",
            CommandTimeout = 30
        };
        var dataStore = new SqlServerDataStore("id", "name", originalConfig);

        var newConfig = new SqlServerConfiguration
        {
            ConnectionString = "Invalid Connection String",
            CommandTimeout = 60
        };

        // Act
        var result = await dataStore.UpdateConfiguration(newConfig);

        // Assert
        dataStore.Configuration.ShouldBe(newConfig);
        dataStore.Location.ShouldBe(newConfig.ConnectionString);
        // Result will be failure because connection string is invalid, but config should still update
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void Metadata_ShouldReturnReadOnlyDictionary()
    {
        // Arrange
        var dataStore = new SqlServerDataStore("id", "name", new SqlServerConfiguration());

        // Act
        var metadata = dataStore.Metadata;

        // Assert
        metadata.ShouldNotBeNull();
        metadata.ShouldBeAssignableTo<IReadOnlyDictionary<string, object>>();
    }

    [Fact]
    public void Constructor_ShouldSetStoreTypeToSqlServer()
    {
        // Arrange & Act
        var dataStore = new SqlServerDataStore("id", "name", new SqlServerConfiguration());

        // Assert
        dataStore.StoreType.ShouldBe(SqlServerDataStoreType.Instance.Name);
    }

    [Fact]
    public async Task TestConnection_ShouldReturnSuccess_WithValidLocalDbConnection()
    {
        // Arrange
        var config = new SqlServerConfiguration
        {
            ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;Connect Timeout=5;"
        };
        var dataStore = new SqlServerDataStore("id", "name", config);

        // Act
        var result = await dataStore.TestConnection();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public async Task DiscoverPaths_ShouldReturnSuccess_WithValidLocalDbConnection()
    {
        // Arrange
        var config = new SqlServerConfiguration
        {
            ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;Connect Timeout=5;"
        };
        var dataStore = new SqlServerDataStore("id", "name", config);

        // Act
        var result = await dataStore.DiscoverPaths();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateConfiguration_ShouldReturnSuccess_WithValidConnection()
    {
        // Arrange
        var originalConfig = new SqlServerConfiguration
        {
            ConnectionString = "Invalid",
            CommandTimeout = 30
        };
        var dataStore = new SqlServerDataStore("id", "name", originalConfig);

        var newConfig = new SqlServerConfiguration
        {
            ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;Connect Timeout=5;",
            CommandTimeout = 60
        };

        // Act
        var result = await dataStore.UpdateConfiguration(newConfig);

        // Assert
        dataStore.Configuration.ShouldBe(newConfig);
        dataStore.Location.ShouldBe(newConfig.ConnectionString);
        result.IsSuccess.ShouldBeTrue();
    }
}
