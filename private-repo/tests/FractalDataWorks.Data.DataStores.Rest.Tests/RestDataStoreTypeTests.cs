using FractalDataWorks.Data.DataStores.Rest;
using Shouldly;

namespace FractalDataWorks.Data.DataStores.Rest.Tests;

public sealed class RestDataStoreTypeTests
{
    [Fact]
    public void Instance_ShouldReturnSingletonInstance()
    {
        // Act
        var instance1 = RestDataStoreType.Instance;
        var instance2 = RestDataStoreType.Instance;

        // Assert
        instance1.ShouldNotBeNull();
        instance2.ShouldNotBeNull();
        instance1.ShouldBe(instance2);
    }

    [Fact]
    public void Instance_ShouldHaveCorrectId()
    {
        // Act
        var instance = RestDataStoreType.Instance;

        // Assert
        instance.Id.ShouldBe(2);
    }

    [Fact]
    public void Instance_ShouldHaveCorrectName()
    {
        // Act
        var instance = RestDataStoreType.Instance;

        // Assert
        instance.Name.ShouldBe("Rest");
    }

    [Fact]
    public void Instance_ShouldHaveCorrectDisplayName()
    {
        // Act
        var instance = RestDataStoreType.Instance;

        // Assert
        instance.DisplayName.ShouldBe("REST API");
    }

    [Fact]
    public void Instance_ShouldHaveCorrectDescription()
    {
        // Act
        var instance = RestDataStoreType.Instance;

        // Assert
        instance.Description.ShouldBe("REST API data store supporting JSON, XML, and custom formats");
    }

    [Fact]
    public void Instance_ShouldSupportRead()
    {
        // Act
        var instance = RestDataStoreType.Instance;

        // Assert
        instance.SupportsRead.ShouldBeTrue();
    }

    [Fact]
    public void Instance_ShouldSupportWrite()
    {
        // Act
        var instance = RestDataStoreType.Instance;

        // Assert
        instance.SupportsWrite.ShouldBeTrue();
    }

    [Fact]
    public void Instance_ShouldNotSupportTransactions()
    {
        // Act
        var instance = RestDataStoreType.Instance;

        // Assert
        instance.SupportsTransactions.ShouldBeFalse();
    }

    [Fact]
    public void Instance_ShouldHaveCorrectCategory()
    {
        // Act
        var instance = RestDataStoreType.Instance;

        // Assert
        instance.Category.ShouldBe("Web");
    }
}
