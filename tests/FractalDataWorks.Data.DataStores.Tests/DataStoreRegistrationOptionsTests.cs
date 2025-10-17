using FractalDataWorks.Data.DataStores;
using FractalDataWorks.Data.DataStores.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace FractalDataWorks.Data.DataStores.Tests;

public sealed class DataStoreRegistrationOptionsTests
{
    [Fact]
    public void Constructor_ShouldInitializeCustomConfigurations()
    {
        // Act
        var options = new DataStoreRegistrationOptions();

        // Assert
        options.CustomConfigurations.ShouldNotBeNull();
        options.CustomConfigurations.ShouldBeEmpty();
    }

    [Fact]
    public void Configure_ShouldAddConfigurationToCustomConfigurations()
    {
        // Arrange
        var options = new DataStoreRegistrationOptions();
        var dataStoreTypeName = "SqlServer";
        Action<IServiceCollection, IDataStoreType> configureAction = (services, type) => { };

        // Act
        options.Configure(dataStoreTypeName, configureAction);

        // Assert
        options.CustomConfigurations.ShouldContainKey(dataStoreTypeName);
        options.CustomConfigurations[dataStoreTypeName].ShouldBe(configureAction);
    }

    [Fact]
    public void Configure_ShouldOverwriteExistingConfiguration()
    {
        // Arrange
        var options = new DataStoreRegistrationOptions();
        var dataStoreTypeName = "SqlServer";
        Action<IServiceCollection, IDataStoreType> firstAction = (services, type) => { };
        Action<IServiceCollection, IDataStoreType> secondAction = (services, type) => { };

        // Act
        options.Configure(dataStoreTypeName, firstAction);
        options.Configure(dataStoreTypeName, secondAction);

        // Assert
        options.CustomConfigurations.ShouldContainKey(dataStoreTypeName);
        options.CustomConfigurations[dataStoreTypeName].ShouldBe(secondAction);
        options.CustomConfigurations.Count.ShouldBe(1);
    }

    [Fact]
    public void Configure_ShouldHandleMultipleDataStoreTypes()
    {
        // Arrange
        var options = new DataStoreRegistrationOptions();
        var sqlServerAction = (IServiceCollection services, IDataStoreType type) => { };
        var restAction = (IServiceCollection services, IDataStoreType type) => { };
        var fileAction = (IServiceCollection services, IDataStoreType type) => { };

        // Act
        options.Configure("SqlServer", sqlServerAction);
        options.Configure("Rest", restAction);
        options.Configure("File", fileAction);

        // Assert
        options.CustomConfigurations.Count.ShouldBe(3);
        options.CustomConfigurations.ShouldContainKey("SqlServer");
        options.CustomConfigurations.ShouldContainKey("Rest");
        options.CustomConfigurations.ShouldContainKey("File");
    }

    [Fact]
    public void Configure_ShouldBeCaseSensitive()
    {
        // Arrange
        var options = new DataStoreRegistrationOptions();
        var lowerCaseAction = (IServiceCollection services, IDataStoreType type) => { };
        var upperCaseAction = (IServiceCollection services, IDataStoreType type) => { };

        // Act
        options.Configure("sqlserver", lowerCaseAction);
        options.Configure("SqlServer", upperCaseAction);

        // Assert
        options.CustomConfigurations.Count.ShouldBe(2);
        options.CustomConfigurations.ShouldContainKey("sqlserver");
        options.CustomConfigurations.ShouldContainKey("SqlServer");
        options.CustomConfigurations["sqlserver"].ShouldBe(lowerCaseAction);
        options.CustomConfigurations["SqlServer"].ShouldBe(upperCaseAction);
    }

    [Fact]
    public void CustomConfigurations_ShouldAllowDirectAccess()
    {
        // Arrange
        var options = new DataStoreRegistrationOptions();
        var dataStoreTypeName = "Test";
        Action<IServiceCollection, IDataStoreType> action = (services, type) => { };

        // Act
        options.CustomConfigurations[dataStoreTypeName] = action;

        // Assert
        options.CustomConfigurations[dataStoreTypeName].ShouldBe(action);
    }

    [Fact]
    public void Configure_ShouldInvokeActionWhenCalled()
    {
        // Arrange
        var options = new DataStoreRegistrationOptions();
        var serviceCollection = new ServiceCollection();
        var mockDataStoreType = new Mock<IDataStoreType>();
        var actionWasInvoked = false;

        Action<IServiceCollection, IDataStoreType> action = (services, type) =>
        {
            actionWasInvoked = true;
        };

        options.Configure("Test", action);

        // Act
        options.CustomConfigurations["Test"](serviceCollection, mockDataStoreType.Object);

        // Assert
        actionWasInvoked.ShouldBeTrue();
    }
}
