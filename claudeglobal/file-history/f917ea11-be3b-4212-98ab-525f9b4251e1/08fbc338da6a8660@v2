using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.ServiceTypes.Tests;

/// <summary>
/// Tests for ServiceTypeBase covering all pathways for 100% coverage.
/// </summary>
public class ServiceTypeBaseTests
{
    // Test interfaces and classes
    private interface ITestService : IGenericService { }

    private class TestConfiguration : IGenericConfiguration
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }

    private interface ITestFactory : IServiceFactory<ITestService> { }

    private interface ITestFactoryWithConfig : IServiceFactory<ITestService, TestConfiguration> { }

    // Two-generic variant (TService, TFactory)
    private sealed class TestServiceType : ServiceTypeBase<ITestService, ITestFactory>
    {
        public TestServiceType(int id, string name, string sectionName, string displayName, string description, string? category = null)
            : base(id, name, sectionName, displayName, description, category) { }

        public override void Register(IServiceCollection services) { }

        public override void Configure(IConfiguration configuration) { }
    }

    // Three-generic variant (TService, TFactory, TConfiguration)
    private sealed class TestServiceTypeWithConfig : ServiceTypeBase<ITestService, ITestFactoryWithConfig, TestConfiguration>
    {
        public TestServiceTypeWithConfig(int id, string name, string sectionName, string displayName, string description, string? category = null)
            : base(id, name, sectionName, displayName, description, category) { }

        public override void Register(IServiceCollection services) { }

        public override void Configure(IConfiguration configuration) { }
    }

    [Fact]
    public void Constructor_InitializesAllProperties()
    {
        var serviceType = new TestServiceType(
            id: 42,
            name: "TestService",
            sectionName: "TestSection",
            displayName: "Test Service Display",
            description: "Test service description",
            category: "TestCategory");

        serviceType.Id.ShouldBe(42);
        serviceType.Name.ShouldBe("TestService");
        serviceType.SectionName.ShouldBe("TestSection");
        serviceType.DisplayName.ShouldBe("Test Service Display");
        serviceType.Description.ShouldBe("Test service description");
        serviceType.Category.ShouldBe("TestCategory");
    }

    [Fact]
    public void Constructor_WithNullCategory_SetsDefaultCategory()
    {
        var serviceType = new TestServiceType(1, "Test", "Section", "Display", "Desc", null);

        serviceType.Category.ShouldBe("Default");
    }

    [Fact]
    public void Constructor_WithoutCategory_SetsDefaultCategory()
    {
        var serviceType = new TestServiceType(1, "Test", "Section", "Display", "Desc");

        serviceType.Category.ShouldBe("Default");
    }

    [Fact]
    public void ServiceType_ReturnsCorrectType()
    {
        var serviceType = new TestServiceType(1, "Test", "Section", "Display", "Desc");

        serviceType.ServiceType.ShouldBe(typeof(ITestService));
    }

    [Fact]
    public void FactoryType_ReturnsCorrectType()
    {
        var serviceType = new TestServiceType(1, "Test", "Section", "Display", "Desc");

        serviceType.FactoryType.ShouldBe(typeof(ITestFactory));
    }

    [Theory]
    [InlineData(0, "Zero")]
    [InlineData(1, "One")]
    [InlineData(-1, "Negative")]
    [InlineData(int.MaxValue, "Max")]
    public void Constructor_WithVariousIds_InitializesCorrectly(int id, string name)
    {
        var serviceType = new TestServiceType(id, name, "Section", "Display", "Desc");

        serviceType.Id.ShouldBe(id);
        serviceType.Name.ShouldBe(name);
    }

    [Fact]
    public void ThreeGenericVariant_ConfigurationType_ReturnsCorrectType()
    {
        var serviceType = new TestServiceTypeWithConfig(1, "Test", "Section", "Display", "Desc");

        serviceType.ConfigurationType.ShouldBe(typeof(TestConfiguration));
    }

    [Fact]
    public void ThreeGenericVariant_ServiceType_ReturnsCorrectType()
    {
        var serviceType = new TestServiceTypeWithConfig(1, "Test", "Section", "Display", "Desc");

        serviceType.ServiceType.ShouldBe(typeof(ITestService));
    }

    [Fact]
    public void ThreeGenericVariant_FactoryType_ReturnsCorrectType()
    {
        var serviceType = new TestServiceTypeWithConfig(1, "Test", "Section", "Display", "Desc");

        serviceType.FactoryType.ShouldBe(typeof(ITestFactoryWithConfig));
    }

    [Fact]
    public void ThreeGenericVariant_InheritsFromTwoGenericVariant()
    {
        var serviceType = new TestServiceTypeWithConfig(1, "Test", "Section", "Display", "Desc");

        serviceType.ShouldBeAssignableTo<ServiceTypeBase<ITestService, ITestFactoryWithConfig>>();
    }

    [Fact]
    public void ThreeGenericVariant_AllPropertiesInitialized()
    {
        var serviceType = new TestServiceTypeWithConfig(
            id: 123,
            name: "ConfigTest",
            sectionName: "ConfigSection",
            displayName: "Config Test Display",
            description: "Config test description",
            category: "ConfigCategory");

        serviceType.Id.ShouldBe(123);
        serviceType.Name.ShouldBe("ConfigTest");
        serviceType.SectionName.ShouldBe("ConfigSection");
        serviceType.DisplayName.ShouldBe("Config Test Display");
        serviceType.Description.ShouldBe("Config test description");
        serviceType.Category.ShouldBe("ConfigCategory");
        serviceType.ConfigurationType.ShouldBe(typeof(TestConfiguration));
    }

    [Fact]
    public void Register_CanBeCalled()
    {
        var serviceType = new TestServiceType(1, "Test", "Section", "Display", "Desc");
        var services = new ServiceCollection();

        Should.NotThrow(() => serviceType.Register(services));
    }

    [Fact]
    public void Configure_CanBeCalled()
    {
        var serviceType = new TestServiceType(1, "Test", "Section", "Display", "Desc");
        var configuration = new Mock<IConfiguration>().Object;

        Should.NotThrow(() => serviceType.Configure(configuration));
    }

    [Fact]
    public void Properties_AreReadOnly()
    {
        var serviceType = new TestServiceType(1, "Test", "Section", "Display", "Desc");

        // Verify properties have getters but not setters (compile-time check)
        var idProperty = typeof(ServiceTypeBase<ITestService, ITestFactory>).GetProperty("Id");
        idProperty!.CanWrite.ShouldBeFalse();

        var nameProperty = typeof(ServiceTypeBase<ITestService, ITestFactory>).GetProperty("Name");
        nameProperty!.CanWrite.ShouldBeFalse();

        var serviceTypeProperty = typeof(ServiceTypeBase<ITestService, ITestFactory>).GetProperty("ServiceType");
        serviceTypeProperty!.CanWrite.ShouldBeFalse();

        var factoryTypeProperty = typeof(ServiceTypeBase<ITestService, ITestFactory>).GetProperty("FactoryType");
        factoryTypeProperty!.CanWrite.ShouldBeFalse();
    }
}
