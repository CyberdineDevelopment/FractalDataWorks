using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Tests;

/// <summary>
/// Tests for ServiceFactory covering all pathways for 100% coverage.
/// </summary>
public class ServiceFactoryTests
{
    public class TestService : IGenericService
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public string ServiceType { get; } = nameof(TestService);
        public bool IsAvailable { get; } = true;
        public string Name { get; }

        public TestService(ILogger<TestService> logger, TestConfiguration configuration)
        {
            Name = configuration.Name;
        }
    }

    public class TestConfiguration : IGenericConfiguration
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }

    private class TestServiceFactory : ServiceFactory<TestService, TestConfiguration>
    {
        public TestServiceFactory(ILogger? logger = null) : base(logger) { }

        public ILogger ExposedLogger => Logger;
    }

    [Fact]
    public void Constructor_WithNullLogger_UsesNullLogger()
    {
        var factory = new TestServiceFactory(null);

        factory.ExposedLogger.ShouldBeOfType<NullLogger>();
    }

    [Fact]
    public void Constructor_WithLogger_UsesProvidedLogger()
    {
        var logger = new Mock<ILogger>().Object;
        var factory = new TestServiceFactory(logger);

        factory.ExposedLogger.ShouldBe(logger);
    }

    [Fact]
    public void Create_WithNullConfiguration_ReturnsFailure()
    {
        var factory = new TestServiceFactory();

        var result = factory.Create(null!);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
    }

    [Fact]
    public void Create_WithValidConfiguration_CreatesService()
    {
        var factory = new TestServiceFactory();
        var config = new TestConfiguration { Name = "TestService" };

        var result = factory.Create(config);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Name.ShouldBe("TestService");
    }

    [Fact]
    public void Create_WithValidConfiguration_ReturnsSuccessMessage()
    {
        var factory = new TestServiceFactory();
        var config = new TestConfiguration { Name = "TestService" };

        var result = factory.Create(config);

        result.CurrentMessage.ShouldContain("Service created successfully");
    }

    [Fact]
    public void CreateGeneric_WithMatchingType_CreatesService()
    {
        var factory = new TestServiceFactory();
        var config = new TestConfiguration { Name = "TestService" };

        var result = factory.Create<TestService>(config);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }

    [Fact]
    public void CreateGeneric_WithInvalidConfiguration_ReturnsFailure()
    {
        var factory = new TestServiceFactory();
        var wrongConfig = new Mock<IGenericConfiguration>().Object;

        var result = factory.Create<TestService>(wrongConfig);

        result.IsFailure.ShouldBeTrue();
        result.CurrentMessage.ShouldContain("Invalid configuration type");
    }

    [Fact]
    public void CreateGeneric_ReturnsTypedResult()
    {
        var factory = new TestServiceFactory();
        var config = new TestConfiguration { Name = "TestService" };

        var result = factory.Create<TestService>(config);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeAssignableTo<TestService>();
    }

    [Fact]
    public void CreateNonGeneric_WithValidConfiguration_CreatesService()
    {
        IServiceFactory factory = new TestServiceFactory();
        var config = new TestConfiguration { Name = "TestService" };

        var result = factory.Create(config);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeAssignableTo<IGenericService>();
    }

    [Fact]
    public void CreateNonGeneric_WithInvalidConfiguration_ReturnsFailure()
    {
        IServiceFactory factory = new TestServiceFactory();
        var wrongConfig = new Mock<IGenericConfiguration>().Object;

        var result = factory.Create(wrongConfig);

        result.IsFailure.ShouldBeTrue();
        result.CurrentMessage.ShouldContain("Invalid configuration type");
    }

    [Fact]
    public void CreateTypedInterface_WithValidConfiguration_CreatesService()
    {
        IServiceFactory<TestService> factory = new TestServiceFactory();
        var config = new TestConfiguration { Name = "TestService" };

        var result = factory.Create(config);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }

    [Fact]
    public void CreateTypedInterface_WithInvalidConfiguration_ReturnsFailure()
    {
        IServiceFactory<TestService> factory = new TestServiceFactory();
        var wrongConfig = new Mock<IGenericConfiguration>().Object;

        var result = factory.Create(wrongConfig);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Create_WithLogger_CallsIsEnabled()
    {
        var mockLogger = new Mock<ILogger>();
        var factory = new TestServiceFactory(mockLogger.Object);
        var config = new TestConfiguration { Name = "TestService" };

        factory.Create(config);

        mockLogger.Verify(x => x.IsEnabled(It.IsAny<LogLevel>()), Times.AtLeastOnce);
    }
}
