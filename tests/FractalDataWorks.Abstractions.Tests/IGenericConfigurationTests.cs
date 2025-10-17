using FractalDataWorks.Configuration.Abstractions;

namespace FractalDataWorks.Abstractions.Tests;

public class IGenericConfigurationTests
{
    private class TestConfiguration : IGenericConfiguration
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string SectionName { get; init; } = string.Empty;
        public bool IsEnabled { get; init; }
    }

    [Fact]
    public void IGenericConfiguration_CanBeImplemented()
    {
        var config = new TestConfiguration
        {
            Id = 1,
            Name = "TestConfig",
            SectionName = "Test",
            IsEnabled = true
        };

        config.Id.ShouldBe(1);
        config.Name.ShouldBe("TestConfig");
        config.SectionName.ShouldBe("Test");
        config.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void IGenericConfiguration_Id_CanBeSet()
    {
        var config = new TestConfiguration { Id = 42 };

        config.Id.ShouldBe(42);
    }

    [Fact]
    public void IGenericConfiguration_Name_CanBeSet()
    {
        var config = new TestConfiguration { Name = "MyConfig" };

        config.Name.ShouldBe("MyConfig");
    }

    [Fact]
    public void IGenericConfiguration_SectionName_CanBeSet()
    {
        var config = new TestConfiguration { SectionName = "AppSettings" };

        config.SectionName.ShouldBe("AppSettings");
    }

    [Fact]
    public void IGenericConfiguration_IsEnabled_CanBeTrue()
    {
        var config = new TestConfiguration { IsEnabled = true };

        config.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void IGenericConfiguration_IsEnabled_CanBeFalse()
    {
        var config = new TestConfiguration { IsEnabled = false };

        config.IsEnabled.ShouldBeFalse();
    }
}
