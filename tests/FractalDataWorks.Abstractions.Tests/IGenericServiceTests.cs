using FractalDataWorks.Abstractions;
using FractalDataWorks.Results;
using Moq;

namespace FractalDataWorks.Abstractions.Tests;

public class IGenericServiceTests
{
    private class TestGenericService : IGenericService
    {
        public string Id { get; init; } = string.Empty;
        public string ServiceType { get; init; } = string.Empty;
        public bool IsAvailable { get; init; }

        public Task<IGenericResult<T>> Execute<T>(IGenericCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IGenericResult> Execute(IGenericCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void IGenericService_CanBeImplemented()
    {
        var service = new TestGenericService
        {
            Id = "test-service",
            ServiceType = "TestService",
            IsAvailable = true
        };

        service.Id.ShouldBe("test-service");
        service.ServiceType.ShouldBe("TestService");
        service.IsAvailable.ShouldBeTrue();
    }

    [Fact]
    public void IGenericService_Id_CanBeSet()
    {
        var service = new TestGenericService { Id = "service-123" };

        service.Id.ShouldBe("service-123");
    }

    [Fact]
    public void IGenericService_ServiceType_CanBeSet()
    {
        var service = new TestGenericService { ServiceType = "DataService" };

        service.ServiceType.ShouldBe("DataService");
    }

    [Fact]
    public void IGenericService_IsAvailable_CanBeTrue()
    {
        var service = new TestGenericService { IsAvailable = true };

        service.IsAvailable.ShouldBeTrue();
    }

    [Fact]
    public void IGenericService_IsAvailable_CanBeFalse()
    {
        var service = new TestGenericService { IsAvailable = false };

        service.IsAvailable.ShouldBeFalse();
    }

    [Fact]
    public async Task IGenericService_Execute_Generic_CanBeCalled()
    {
        var mockService = new Mock<IGenericService>();
        var mockCommand = new Mock<IGenericCommand>();
        var mockResult = new Mock<IGenericResult<string>>();

        mockResult.Setup(r => r.IsSuccess).Returns(true);
        mockResult.Setup(r => r.Value).Returns("Success");
        mockService.Setup(s => s.Execute<string>(It.IsAny<IGenericCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResult.Object);

        var result = await mockService.Object.Execute<string>(mockCommand.Object, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("Success");
    }

    [Fact]
    public async Task IGenericService_Execute_NonGeneric_CanBeCalled()
    {
        var mockService = new Mock<IGenericService>();
        var mockCommand = new Mock<IGenericCommand>();
        var mockResult = new Mock<IGenericResult>();

        mockResult.Setup(r => r.IsSuccess).Returns(true);
        mockService.Setup(s => s.Execute(It.IsAny<IGenericCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResult.Object);

        var result = await mockService.Object.Execute(mockCommand.Object, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task IGenericService_Execute_CanUseCancellationToken()
    {
        var mockService = new Mock<IGenericService>();
        var mockCommand = new Mock<IGenericCommand>();
        var cts = new CancellationTokenSource();
        var mockResult = new Mock<IGenericResult>();

        mockService.Setup(s => s.Execute(mockCommand.Object, cts.Token))
            .ReturnsAsync(mockResult.Object);

        var result = await mockService.Object.Execute(mockCommand.Object, cts.Token);

        result.ShouldNotBeNull();
    }
}
