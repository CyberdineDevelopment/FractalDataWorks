using FractalDataWorks.Abstractions;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using Moq;

namespace FractalDataWorks.Abstractions.Tests;

public class IServiceFactoryTests
{
    [Fact]
    public void IServiceFactory_Create_Generic_CanBeCalled()
    {
        var mockFactory = new Mock<IServiceFactory>();
        var mockConfig = new Mock<IGenericConfiguration>();
        var mockResult = new Mock<IGenericResult<IGenericService>>();

        mockFactory.Setup(f => f.Create<IGenericService>(mockConfig.Object))
            .Returns(mockResult.Object);

        var result = mockFactory.Object.Create<IGenericService>(mockConfig.Object);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void IServiceFactory_Create_NonGeneric_CanBeCalled()
    {
        var mockFactory = new Mock<IServiceFactory>();
        var mockConfig = new Mock<IGenericConfiguration>();
        var mockResult = new Mock<IGenericResult<IGenericService>>();

        mockFactory.Setup(f => f.Create(mockConfig.Object))
            .Returns(mockResult.Object);

        var result = mockFactory.Object.Create(mockConfig.Object);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void IServiceFactory_Generic_InheritsFromBase()
    {
        var mockFactory = new Mock<IServiceFactory<IGenericService>>();

        IServiceFactory baseFactory = mockFactory.Object;

        baseFactory.ShouldNotBeNull();
    }

    [Fact]
    public void IServiceFactory_Generic_Create_CanBeCalled()
    {
        var mockFactory = new Mock<IServiceFactory<IGenericService>>();
        var mockConfig = new Mock<IGenericConfiguration>();
        var mockResult = new Mock<IGenericResult<IGenericService>>();

        mockFactory.Setup(f => f.Create(mockConfig.Object))
            .Returns(mockResult.Object);

        var result = mockFactory.Object.Create(mockConfig.Object);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void IServiceFactory_WithConfiguration_InheritsFromGeneric()
    {
        var mockFactory = new Mock<IServiceFactory<IGenericService, IGenericConfiguration>>();

        IServiceFactory<IGenericService> genericFactory = mockFactory.Object;

        genericFactory.ShouldNotBeNull();
    }

    [Fact]
    public void IServiceFactory_WithConfiguration_Create_CanBeCalled()
    {
        var mockFactory = new Mock<IServiceFactory<IGenericService, IGenericConfiguration>>();
        var mockConfig = new Mock<IGenericConfiguration>();
        var mockResult = new Mock<IGenericResult<IGenericService>>();

        mockFactory.Setup(f => f.Create(mockConfig.Object))
            .Returns(mockResult.Object);

        var result = mockFactory.Object.Create(mockConfig.Object);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void IServiceFactory_Create_ReturnsSuccessResult()
    {
        var mockFactory = new Mock<IServiceFactory>();
        var mockConfig = new Mock<IGenericConfiguration>();
        var mockService = new Mock<IGenericService>();
        var mockResult = new Mock<IGenericResult<IGenericService>>();

        mockResult.Setup(r => r.IsSuccess).Returns(true);
        mockResult.Setup(r => r.Value).Returns(mockService.Object);
        mockFactory.Setup(f => f.Create<IGenericService>(mockConfig.Object))
            .Returns(mockResult.Object);

        var result = mockFactory.Object.Create<IGenericService>(mockConfig.Object);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }

    [Fact]
    public void IServiceFactory_Create_ReturnsFailureResult()
    {
        var mockFactory = new Mock<IServiceFactory>();
        var mockConfig = new Mock<IGenericConfiguration>();
        var mockResult = new Mock<IGenericResult<IGenericService>>();

        mockResult.Setup(r => r.IsSuccess).Returns(false);
        mockResult.Setup(r => r.IsFailure).Returns(true);
        mockFactory.Setup(f => f.Create<IGenericService>(mockConfig.Object))
            .Returns(mockResult.Object);

        var result = mockFactory.Object.Create<IGenericService>(mockConfig.Object);

        result.IsFailure.ShouldBeTrue();
    }
}
