using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Tests;

/// <summary>
/// Tests for ServiceFactoryProvider covering all pathways for 100% coverage.
/// </summary>
public class ServiceFactoryProviderTests
{
    public interface ITestService : IGenericService { }

    public class TestConfiguration : IGenericConfiguration
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }

    [Fact]
    public void Constructor_InitializesEmptyRegistrations()
    {
        var provider = new ServiceFactoryProvider();

        var typeNames = provider.GetRegisteredTypeNames();
        typeNames.ShouldBeEmpty();
    }

    [Fact]
    public void RegisterFactory_TwoParameters_RegistersSuccessfully()
    {
        var provider = new ServiceFactoryProvider();
        var factory = new Mock<IServiceFactory>().Object;

        var result = provider.RegisterFactory("TestService", factory);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void RegisterFactory_ThreeParameters_RegistersSuccessfully()
    {
        var provider = new ServiceFactoryProvider();
        var factory = new Mock<IServiceFactory>().Object;
        var lifetime = ServiceLifetimes.Scoped;

        var result = provider.RegisterFactory("TestService", factory, lifetime);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void RegisterFactory_WithNullTypeName_ReturnsFailure()
    {
        var provider = new ServiceFactoryProvider();
        var factory = new Mock<IServiceFactory>().Object;

        var result = provider.RegisterFactory(null!, factory, ServiceLifetimes.Scoped);

        result.IsFailure.ShouldBeTrue();
        result.CurrentMessage.ShouldContain("cannot be null or empty");
    }

    [Fact]
    public void RegisterFactory_WithEmptyTypeName_ReturnsFailure()
    {
        var provider = new ServiceFactoryProvider();
        var factory = new Mock<IServiceFactory>().Object;

        var result = provider.RegisterFactory(string.Empty, factory, ServiceLifetimes.Scoped);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void RegisterFactory_WithWhitespaceTypeName_ReturnsFailure()
    {
        var provider = new ServiceFactoryProvider();
        var factory = new Mock<IServiceFactory>().Object;

        var result = provider.RegisterFactory("   ", factory, ServiceLifetimes.Scoped);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void RegisterFactory_WithNullFactory_ReturnsFailure()
    {
        var provider = new ServiceFactoryProvider();

        var result = provider.RegisterFactory("TestService", null!, ServiceLifetimes.Scoped);

        result.IsFailure.ShouldBeTrue();
        result.CurrentMessage.ShouldContain("Factory cannot be null");
    }

    [Fact]
    public void RegisterFactory_WithNullLifetime_ReturnsFailure()
    {
        var provider = new ServiceFactoryProvider();
        var factory = new Mock<IServiceFactory>().Object;

        var result = provider.RegisterFactory("TestService", factory, null!);

        result.IsFailure.ShouldBeTrue();
        result.CurrentMessage.ShouldContain("Lifetime cannot be null");
    }

    [Fact]
    public void RegisterFactory_DuplicateTypeName_ReturnsFailure()
    {
        var provider = new ServiceFactoryProvider();
        var factory1 = new Mock<IServiceFactory>().Object;
        var factory2 = new Mock<IServiceFactory>().Object;

        provider.RegisterFactory("TestService", factory1);
        var result = provider.RegisterFactory("TestService", factory2);

        result.IsFailure.ShouldBeTrue();
        result.CurrentMessage.ShouldContain("already registered");
    }

    [Fact]
    public void RegisterFactory_IsCaseInsensitive()
    {
        var provider = new ServiceFactoryProvider();
        var factory1 = new Mock<IServiceFactory>().Object;
        var factory2 = new Mock<IServiceFactory>().Object;

        provider.RegisterFactory("TestService", factory1);
        var result = provider.RegisterFactory("testservice", factory2);

        result.IsFailure.ShouldBeTrue();
        result.CurrentMessage.ShouldContain("already registered");
    }

    [Fact]
    public void GetFactory_ReturnsRegisteredFactory()
    {
        var provider = new ServiceFactoryProvider();
        var factory = new Mock<IServiceFactory>().Object;
        provider.RegisterFactory("TestService", factory);

        var result = provider.GetFactory("TestService");

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(factory);
    }

    [Fact]
    public void GetFactory_WithNullTypeName_ReturnsFailure()
    {
        var provider = new ServiceFactoryProvider();

        var result = provider.GetFactory(null!);

        result.IsFailure.ShouldBeTrue();
        result.CurrentMessage.ShouldContain("cannot be null or empty");
    }

    [Fact]
    public void GetFactory_WithEmptyTypeName_ReturnsFailure()
    {
        var provider = new ServiceFactoryProvider();

        var result = provider.GetFactory(string.Empty);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void GetFactory_WithWhitespaceTypeName_ReturnsFailure()
    {
        var provider = new ServiceFactoryProvider();

        var result = provider.GetFactory("   ");

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void GetFactory_NotRegistered_ReturnsFailure()
    {
        var provider = new ServiceFactoryProvider();

        var result = provider.GetFactory("NonExistent");

        result.IsFailure.ShouldBeTrue();
        result.CurrentMessage.ShouldContain("No factory registered");
    }

    [Fact]
    public void GetFactory_NotRegistered_IncludesAvailableTypes()
    {
        var provider = new ServiceFactoryProvider();
        provider.RegisterFactory("Service1", new Mock<IServiceFactory>().Object);
        provider.RegisterFactory("Service2", new Mock<IServiceFactory>().Object);

        var result = provider.GetFactory("NonExistent");

        result.CurrentMessage.ShouldContain("Available types:");
        result.CurrentMessage.ShouldContain("Service1");
        result.CurrentMessage.ShouldContain("Service2");
    }

    [Fact]
    public void GetFactory_IsCaseInsensitive()
    {
        var provider = new ServiceFactoryProvider();
        var factory = new Mock<IServiceFactory>().Object;
        provider.RegisterFactory("TestService", factory);

        var result = provider.GetFactory("testservice");

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(factory);
    }

    [Fact]
    public void GetFactoryGeneric_ReturnsTypedFactory()
    {
        var provider = new ServiceFactoryProvider();
        var factory = new Mock<IServiceFactory<ITestService, TestConfiguration>>().Object;
        provider.RegisterFactory("TestService", factory);

        var result = provider.GetFactory<ITestService, TestConfiguration>("TestService");

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(factory);
    }

    [Fact]
    public void GetFactoryGeneric_WithWrongType_ReturnsFailure()
    {
        var provider = new ServiceFactoryProvider();
        var factory = new Mock<IServiceFactory>().Object;
        provider.RegisterFactory("TestService", factory);

        var result = provider.GetFactory<ITestService, TestConfiguration>("TestService");

        result.IsFailure.ShouldBeTrue();
        result.CurrentMessage.ShouldContain("does not implement");
    }

    [Fact]
    public void GetFactoryGeneric_NotRegistered_ReturnsFailure()
    {
        var provider = new ServiceFactoryProvider();

        var result = provider.GetFactory<ITestService, TestConfiguration>("NonExistent");

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void GetRegisteredTypeNames_ReturnsAllRegisteredNames()
    {
        var provider = new ServiceFactoryProvider();
        provider.RegisterFactory("Service1", new Mock<IServiceFactory>().Object);
        provider.RegisterFactory("Service2", new Mock<IServiceFactory>().Object);
        provider.RegisterFactory("Service3", new Mock<IServiceFactory>().Object);

        var typeNames = provider.GetRegisteredTypeNames().ToList();

        typeNames.Count.ShouldBe(3);
        typeNames.ShouldContain("Service1");
        typeNames.ShouldContain("Service2");
        typeNames.ShouldContain("Service3");
    }

    [Fact]
    public void GetRegisteredTypeNames_EmptyProvider_ReturnsEmpty()
    {
        var provider = new ServiceFactoryProvider();

        var typeNames = provider.GetRegisteredTypeNames();

        typeNames.ShouldBeEmpty();
    }

    [Fact]
    public void GetRegistration_ReturnsFactoryRegistration()
    {
        var provider = new ServiceFactoryProvider();
        var factory = new Mock<IServiceFactory>().Object;
        var lifetime = ServiceLifetimes.Singleton;
        provider.RegisterFactory("TestService", factory, lifetime);

        var result = provider.GetRegistration("TestService");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Factory.ShouldBe(factory);
        result.Value.Lifetime.ShouldBe(lifetime);
        result.Value.TypeName.ShouldBe("TestService");
    }

    [Fact]
    public void GetRegistration_NotFound_ReturnsFailure()
    {
        var provider = new ServiceFactoryProvider();

        var result = provider.GetRegistration("NonExistent");

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void GetRegistration_WithNullTypeName_ReturnsFailure()
    {
        var provider = new ServiceFactoryProvider();

        var result = provider.GetRegistration(null!);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task ThreadSafety_ConcurrentRegistrations()
    {
        var provider = new ServiceFactoryProvider();
        var tasks = new Task[100];

        for (int i = 0; i < 100; i++)
        {
            int index = i;
            tasks[i] = Task.Run(() =>
            {
                provider.RegisterFactory($"Service{index}", new Mock<IServiceFactory>().Object);
            }, TestContext.Current.CancellationToken);
        }

        await Task.WhenAll(tasks);

        provider.GetRegisteredTypeNames().ToList().Count.ShouldBe(100);
    }
}
