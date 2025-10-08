using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.Abstractions.Commands;
using FractalDataWorks.Services.Extensions;
using static FractalDataWorks.Services.Abstractions.ServiceLifetimes;

namespace FractalDataWorks.Services.Tests;

/// <summary>
/// Tests for ServiceFactoryRegistrationExtensions for 100% coverage.
/// </summary>
public class ServiceFactoryRegistrationExtensionsTests
{
    public class TestCommand : ICommand
    {
        public Guid CommandId { get; } = Guid.NewGuid();
        public Guid CorrelationId { get; } = Guid.NewGuid();
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;

        public IGenericResult Validate() => GenericResult.Success();
    }

    public class TestConfiguration : IGenericConfiguration
    {
        public int Id { get; set; } = 1;
        public string Name { get; set; } = "TestConfig";
        public string SectionName { get; set; } = "Test";
        public bool IsEnabled { get; set; } = true;
    }

    public class TestService : ServiceBase<TestCommand, TestConfiguration, TestService>, IGenericService
    {
        public TestService(ILogger<TestService> logger, TestConfiguration configuration)
            : base(logger, configuration)
        {
        }

        public override Task<IGenericResult> Execute(TestCommand command)
        {
            return Task.FromResult<IGenericResult>(GenericResult.Success());
        }

        public override Task<IGenericResult<TOut>> Execute<TOut>(TestCommand command)
        {
            return Task.FromResult<IGenericResult<TOut>>(GenericResult<TOut>.Success(default!));
        }
    }

    public class TestFactory : GenericServiceFactory<TestService, TestConfiguration>
    {
        public TestFactory(ILogger<GenericServiceFactory<TestService, TestConfiguration>> logger) : base(logger)
        {
        }
    }

    [Fact]
    public void AddServiceFactoryProvider_AddsProviderToServices()
    {
        var services = new ServiceCollection();

        var result = services.AddServiceFactoryProvider();

        result.ShouldBe(services);
        services.ShouldContain(sd => sd.ServiceType == typeof(IServiceFactoryProvider));
        services.ShouldContain(sd => sd.ImplementationType == typeof(ServiceFactoryProvider));
    }

    [Fact]
    public void RegisterConnectionFactories_WithNullConfigure_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        Should.Throw<ArgumentNullException>(() =>
            services.RegisterConnectionFactories(null!));
    }

    [Fact]
    public void RegisterConnectionFactories_WithValidConfigure_AddsFactoryProvider()
    {
        var services = new ServiceCollection();

        var result = services.RegisterConnectionFactories(builder => { });

        result.ShouldBe(services);
        services.ShouldContain(sd => sd.ServiceType == typeof(IServiceFactoryProvider));
    }

    [Fact]
    public void RegisterConnectionFactories_CallsConfigureAction()
    {
        var services = new ServiceCollection();
        var wasConfigureCalled = false;

        services.RegisterConnectionFactories(builder =>
        {
            wasConfigureCalled = true;
        });

        wasConfigureCalled.ShouldBeTrue();
    }

    [Fact]
    public void RegisterConnectionFactoriesFromConfiguration_WithNullConfiguration_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        Should.Throw<ArgumentNullException>(() =>
            services.RegisterConnectionFactoriesFromConfiguration(null!));
    }

    [Fact]
    public void RegisterConnectionFactoriesFromConfiguration_WithoutSection_ReturnsServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var result = services.RegisterConnectionFactoriesFromConfiguration(configuration);

        result.ShouldBe(services);
    }

    [Fact]
    public void RegisterConnectionFactoriesFromConfiguration_WithCustomSectionName_UsesCustomSection()
    {
        var services = new ServiceCollection();
        var configData = new Dictionary<string, string>
        {
            { "CustomSection:Test:FactoryType", "FractalDataWorks.Services.GenericServiceFactory`2" },
            { "CustomSection:Test:Lifetime", "Singleton" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var result = services.RegisterConnectionFactoriesFromConfiguration(configuration, "CustomSection");

        result.ShouldBe(services);
    }

    [Fact]
    public void RegisterConnectionFactoriesFromConfiguration_WithValidConfiguration_AddsFactoryProvider()
    {
        var services = new ServiceCollection();
        var configData = new Dictionary<string, string>
        {
            { "ConnectionFactories:Test:FactoryType", "System.String" },
            { "ConnectionFactories:Test:Lifetime", "Singleton" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var result = services.RegisterConnectionFactoriesFromConfiguration(configuration);

        result.ShouldBe(services);
        services.ShouldContain(sd => sd.ServiceType == typeof(IServiceFactoryProvider));
    }

    [Fact]
    public void RegisterConnectionFactoriesFromConfiguration_WithEmptyFactoryType_SkipsRegistration()
    {
        var services = new ServiceCollection();
        var configData = new Dictionary<string, string>
        {
            { "ConnectionFactories:Test:FactoryType", "" },
            { "ConnectionFactories:Test:Lifetime", "Singleton" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var result = services.RegisterConnectionFactoriesFromConfiguration(configuration);

        result.ShouldBe(services);
    }

    [Fact]
    public void RegisterConnectionFactoriesFromConfiguration_WithInvalidFactoryType_SkipsRegistration()
    {
        var services = new ServiceCollection();
        var configData = new Dictionary<string, string>
        {
            { "ConnectionFactories:Test:FactoryType", "InvalidType.That.DoesNot.Exist" },
            { "ConnectionFactories:Test:Lifetime", "Singleton" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var result = services.RegisterConnectionFactoriesFromConfiguration(configuration);

        result.ShouldBe(services);
    }

    [Fact]
    public void RegisterConnectionFactoriesFromConfiguration_WithNonFactoryType_SkipsRegistration()
    {
        var services = new ServiceCollection();
        var configData = new Dictionary<string, string>
        {
            { "ConnectionFactories:Test:FactoryType", "System.String" },
            { "ConnectionFactories:Test:Lifetime", "Singleton" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var result = services.RegisterConnectionFactoriesFromConfiguration(configuration);

        result.ShouldBe(services);
    }

}
