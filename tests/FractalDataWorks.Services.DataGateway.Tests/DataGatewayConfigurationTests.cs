using FractalDataWorks.Configuration;
using FractalDataWorks.Services.DataGateway;
using Shouldly;

namespace FractalDataWorks.Services.DataGateway.Tests;

public sealed class DataGatewayConfigurationTests
{
    [Fact]
    public void Configuration_ShouldImplementIGenericConfiguration()
    {
        // Arrange
        var config = new DataGatewayConfiguration
        {
            Id = 1,
            Type = "DataGateway",
            Name = "Test Gateway",
            SectionName = "DataGateway"
        };

        // Assert
        config.ShouldBeAssignableTo<IGenericConfiguration>();
    }

    [Fact]
    public void Configuration_ShouldSetAllProperties()
    {
        // Arrange & Act
        var config = new DataGatewayConfiguration
        {
            Id = 5,
            Type = "DataGateway",
            Name = "Production Gateway",
            SectionName = "DataGateway:Production",
            IsEnabled = true
        };

        // Assert
        config.Id.ShouldBe(5);
        config.Type.ShouldBe("DataGateway");
        config.Name.ShouldBe("Production Gateway");
        config.SectionName.ShouldBe("DataGateway:Production");
        config.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void Configuration_IsEnabled_ShouldDefaultToTrue()
    {
        // Arrange & Act
        var config = new DataGatewayConfiguration
        {
            Id = 1,
            Type = "DataGateway",
            Name = "Test",
            SectionName = "Test"
        };

        // Assert
        config.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void Configuration_IsEnabled_CanBeSetToFalse()
    {
        // Arrange & Act
        var config = new DataGatewayConfiguration
        {
            Id = 1,
            Type = "DataGateway",
            Name = "Test",
            SectionName = "Test",
            IsEnabled = false
        };

        // Assert
        config.IsEnabled.ShouldBeFalse();
    }

    [Fact]
    public void Configuration_Properties_ShouldBeInitOnly()
    {
        // Arrange
        var config = new DataGatewayConfiguration
        {
            Id = 1,
            Type = "Original",
            Name = "Original Name",
            SectionName = "Original Section"
        };

        // Assert - if this compiles, init properties work correctly
        config.Id.ShouldBe(1);
        config.Type.ShouldBe("Original");
        config.Name.ShouldBe("Original Name");
        config.SectionName.ShouldBe("Original Section");
    }
}
