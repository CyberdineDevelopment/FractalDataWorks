using FractalDataWorks.Data.DataStores.SqlServer;
using Shouldly;

namespace FractalDataWorks.Data.DataStores.SqlServer.Tests;

public sealed class SqlServerDataStoreTypeTests
{
    [Fact]
    public void Instance_ShouldReturnSingletonInstance()
    {
        // Act
        var instance1 = SqlServerDataStoreType.Instance;
        var instance2 = SqlServerDataStoreType.Instance;

        // Assert
        instance1.ShouldNotBeNull();
        instance2.ShouldNotBeNull();
        instance1.ShouldBe(instance2);
    }

    [Fact]
    public void Instance_ShouldHaveCorrectId()
    {
        // Act
        var instance = SqlServerDataStoreType.Instance;

        // Assert
        instance.Id.ShouldBe(1);
    }

    [Fact]
    public void Instance_ShouldHaveCorrectName()
    {
        // Act
        var instance = SqlServerDataStoreType.Instance;

        // Assert
        instance.Name.ShouldBe("SqlServer");
    }

    [Fact]
    public void Instance_ShouldHaveCorrectDisplayName()
    {
        // Act
        var instance = SqlServerDataStoreType.Instance;

        // Assert
        instance.DisplayName.ShouldBe("SQL Server");
    }

    [Fact]
    public void Instance_ShouldHaveCorrectDescription()
    {
        // Act
        var instance = SqlServerDataStoreType.Instance;

        // Assert
        instance.Description.ShouldBe("Microsoft SQL Server relational database supporting T-SQL, JSON, and XML");
    }

    [Fact]
    public void Instance_ShouldSupportRead()
    {
        // Act
        var instance = SqlServerDataStoreType.Instance;

        // Assert
        instance.SupportsRead.ShouldBeTrue();
    }

    [Fact]
    public void Instance_ShouldSupportWrite()
    {
        // Act
        var instance = SqlServerDataStoreType.Instance;

        // Assert
        instance.SupportsWrite.ShouldBeTrue();
    }

    [Fact]
    public void Instance_ShouldSupportTransactions()
    {
        // Act
        var instance = SqlServerDataStoreType.Instance;

        // Assert
        instance.SupportsTransactions.ShouldBeTrue();
    }

    [Fact]
    public void Instance_ShouldHaveCorrectCategory()
    {
        // Act
        var instance = SqlServerDataStoreType.Instance;

        // Assert
        instance.Category.ShouldBe("Database");
    }
}
