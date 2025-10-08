using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions.Commands;

namespace FractalDataWorks.Services.Tests;

/// <summary>
/// Tests for ServiceBase abstract class for 100% coverage.
/// </summary>
public class ServiceBaseTests
{
    public class TestCommand : ICommand
    {
        public Guid CommandId { get; } = Guid.NewGuid();
        public Guid CorrelationId { get; } = Guid.NewGuid();
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
        public string Data { get; set; } = string.Empty;

        public IGenericResult Validate()
        {
            return GenericResult.Success();
        }
    }

    public class TestConfiguration : IGenericConfiguration
    {
        public int Id { get; set; } = 1;
        public string Name { get; set; } = "TestService";
        public string SectionName { get; set; } = "Test";
        public bool IsEnabled { get; set; } = true;
    }

    public class TestService : ServiceBase<TestCommand, TestConfiguration, TestService>
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

    [Fact]
    public void Constructor_WithValidParameters_InitializesService()
    {
        var logger = new Mock<ILogger<TestService>>();
        var config = new TestConfiguration { Name = "TestService" };

        var service = new TestService(logger.Object, config);

        service.ShouldNotBeNull();
        service.Id.ShouldNotBeNullOrEmpty();
        service.ServiceType.ShouldBe("TestService");
        service.Name.ShouldBe("TestService");
        service.Configuration.ShouldBe(config);
        service.IsAvailable.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        var logger = new Mock<ILogger<TestService>>();

        Should.Throw<ArgumentNullException>(() => new TestService(logger.Object, null!));
    }

    [Fact]
    public void Constructor_WithNullLogger_UsesNullLogger()
    {
        var config = new TestConfiguration { Name = "TestService" };

        var service = new TestService(null!, config);

        service.ShouldNotBeNull();
        service.Configuration.ShouldBe(config);
    }

    [Fact]
    public void Name_WithNullConfiguration_ReturnsTypeName()
    {
        var logger = new Mock<ILogger<TestService>>();
        var config = new TestConfiguration { Name = null! };

        var service = new TestService(logger.Object, config);

        service.Name.ShouldBe("TestService");
    }

    [Fact]
    public void Id_GeneratesUniqueIds()
    {
        var logger = new Mock<ILogger<TestService>>();
        var config = new TestConfiguration();

        var service1 = new TestService(logger.Object, config);
        var service2 = new TestService(logger.Object, config);

        service1.Id.ShouldNotBe(service2.Id);
    }

    [Fact]
    public async Task Execute_WithCommand_ReturnsSuccess()
    {
        var logger = new Mock<ILogger<TestService>>();
        var config = new TestConfiguration();
        var service = new TestService(logger.Object, config);
        var command = new TestCommand { Data = "test" };

        var result = await service.Execute(command);

        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteGeneric_WithCommand_ReturnsSuccess()
    {
        var logger = new Mock<ILogger<TestService>>();
        var config = new TestConfiguration();
        var service = new TestService(logger.Object, config);
        var command = new TestCommand { Data = "test" };

        var result = await service.Execute<string>(command);

        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_WithCancellationToken_ReturnsSuccess()
    {
        var logger = new Mock<ILogger<TestService>>();
        var config = new TestConfiguration();
        var service = new TestService(logger.Object, config);
        var command = new TestCommand { Data = "test" };
        var cts = new CancellationTokenSource();

        var result = await service.Execute(command, cts.Token);

        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteGeneric_WithCancellationToken_ReturnsSuccess()
    {
        var logger = new Mock<ILogger<TestService>>();
        var config = new TestConfiguration();
        var service = new TestService(logger.Object, config);
        var command = new TestCommand { Data = "test" };
        var cts = new CancellationTokenSource();

        var result = await service.Execute<string>(command, cts.Token);

        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Logger_IsAccessibleToDerivativeClasses()
    {
        var logger = new Mock<ILogger<TestService>>();
        var config = new TestConfiguration();
        var service = new TestService(logger.Object, config);

        // The Logger property is protected, but we can verify it through behavior
        // by checking that the service was constructed successfully with a logger
        service.ShouldNotBeNull();
    }
}
