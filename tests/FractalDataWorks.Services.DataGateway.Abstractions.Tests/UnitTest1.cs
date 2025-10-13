using FractalDataWorks.Services.Data.Abstractions;
using Shouldly;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Tests;

public sealed class IDataGatewayTests
{
    [Fact]
    public void IDataGateway_ShouldBeInterface()
    {
        // Arrange
        var type = typeof(IDataGateway);

        // Assert
        type.IsInterface.ShouldBeTrue();
    }

    [Fact]
    public void IDataGateway_ShouldHaveExecuteMethod()
    {
        // Arrange
        var type = typeof(IDataGateway);

        // Act
        var method = type.GetMethod("Execute");

        // Assert
        method.ShouldNotBeNull();
        method.IsGenericMethod.ShouldBeTrue();
    }
}
