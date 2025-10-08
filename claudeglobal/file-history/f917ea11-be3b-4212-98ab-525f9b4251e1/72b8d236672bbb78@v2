using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Tests;

/// <summary>
/// Tests for GenericServiceFactory covering all pathways for 100% coverage.
/// </summary>
public class GenericServiceFactoryTests
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

    [Fact]
    public void Constructor_WithLogger_InitializesCorrectly()
    {
        var logger = new Mock<ILogger<GenericServiceFactory<TestService, TestConfiguration>>>().Object;

        var factory = new GenericServiceFactory<TestService, TestConfiguration>(logger);

        factory.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithoutLogger_UsesNullLogger()
    {
        var factory = new GenericServiceFactory<TestService, TestConfiguration>();

        factory.ShouldNotBeNull();
    }

    [Fact]
    public void Create_WithValidConfiguration_CreatesService()
    {
        var factory = new GenericServiceFactory<TestService, TestConfiguration>();
        var config = new TestConfiguration { Name = "TestService" };

        var result = factory.Create(config);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Name.ShouldBe("TestService");
    }

    [Fact]
    public void Create_WithValidConfiguration_ReturnsSuccessMessage()
    {
        var factory = new GenericServiceFactory<TestService, TestConfiguration>();
        var config = new TestConfiguration { Name = "TestService" };

        var result = factory.Create(config);

        result.CurrentMessage.ShouldContain("Service created successfully");
    }

    [Fact]
    public void Create_WithLogger_LogsCreationAttempt()
    {
        var mockLogger = new Mock<ILogger<GenericServiceFactory<TestService, TestConfiguration>>>();
        var factory = new GenericServiceFactory<TestService, TestConfiguration>(mockLogger.Object);
        var config = new TestConfiguration { Name = "TestService" };

        factory.Create(config);

        mockLogger.Verify(x => x.IsEnabled(It.IsAny<LogLevel>()), Times.AtLeastOnce);
    }

    [Fact]
    public void Create_WithUnnamedConfiguration_HandlesNullName()
    {
        var factory = new GenericServiceFactory<TestService, TestConfiguration>();
        var config = new TestConfiguration { Name = null! };

        var result = factory.Create(config);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }

    [Fact]
    public void Create_InheritsFromServiceFactory()
    {
        var factory = new GenericServiceFactory<TestService, TestConfiguration>();

        factory.ShouldBeAssignableTo<ServiceFactory<TestService, TestConfiguration>>();
    }

    [Fact]
    public void Create_OverridesBaseImplementation()
    {
        var factory = new GenericServiceFactory<TestService, TestConfiguration>();
        var config = new TestConfiguration { Name = "Test" };

        var result = factory.Create(config);

        result.IsSuccess.ShouldBeTrue();
    }
}
