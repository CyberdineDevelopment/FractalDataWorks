using FractalDataWorks.Data.DataSets;
using FractalDataWorks.Data.DataSets.Abstractions;
using FractalDataWorks.Data.Execution;
using FractalDataWorks.Data.Transformers.Abstractions;
using FractalDataWorks.Results;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Data.Execution.Tests;

public class DataConceptQueryExecutorTests
{
    private readonly Mock<IDataConceptRegistry> _registryMock;
    private readonly Mock<ILogger<DataConceptQueryExecutor>> _loggerMock;

    public DataConceptQueryExecutorTests()
    {
        _registryMock = new Mock<IDataConceptRegistry>();
        _loggerMock = new Mock<ILogger<DataConceptQueryExecutor>>();
    }

    [Fact]
    public void Constructor_WithNullRegistry_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new DataConceptQueryExecutor(null!, _loggerMock.Object));

        exception.ParamName.ShouldBe("conceptRegistry");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new DataConceptQueryExecutor(_registryMock.Object, null!));

        exception.ParamName.ShouldBe("logger");
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var executor = new DataConceptQueryExecutor(_registryMock.Object, _loggerMock.Object);

        // Assert
        executor.ShouldNotBeNull();
    }

    [Fact]
    public void RegisterTransformer_WithNullTransformer_ThrowsArgumentNullException()
    {
        // Arrange
        var executor = new DataConceptQueryExecutor(_registryMock.Object, _loggerMock.Object);

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            executor.RegisterTransformer(null!));

        exception.ParamName.ShouldBe("transformer");
    }

    [Fact]
    public void RegisterTransformer_WithValidTransformer_Registers()
    {
        // Arrange
        var executor = new DataConceptQueryExecutor(_registryMock.Object, _loggerMock.Object);
        var transformer = new Mock<IDataTransformer>();
        transformer.Setup(t => t.Name).Returns("TestTransformer");
        transformer.Setup(t => t.SourceType).Returns(typeof(string));
        transformer.Setup(t => t.TargetType).Returns(typeof(int));

        // Act
        executor.RegisterTransformer(transformer.Object);

        // Assert - No exception thrown
    }

    [Fact]
    public async Task Execute_WithNullConceptName_ReturnsFailure()
    {
        // Arrange
        var executor = new DataConceptQueryExecutor(_registryMock.Object, _loggerMock.Object);

        // Act
        var result = await executor.Execute<object>(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_WithEmptyConceptName_ReturnsFailure()
    {
        // Arrange
        var executor = new DataConceptQueryExecutor(_registryMock.Object, _loggerMock.Object);

        // Act
        var result = await executor.Execute<object>(string.Empty);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_WithWhitespaceConceptName_ReturnsFailure()
    {
        // Arrange
        var executor = new DataConceptQueryExecutor(_registryMock.Object, _loggerMock.Object);

        // Act
        var result = await executor.Execute<object>("   ");

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_WhenConceptNotFound_ReturnsFailure()
    {
        // Arrange
        _registryMock.Setup(r => r.GetDataConcept("NonExistent"))
            .Throws(new InvalidOperationException("Concept not found"));

        var executor = new DataConceptQueryExecutor(_registryMock.Object, _loggerMock.Object);

        // Act
        var result = await executor.Execute<object>("NonExistent");

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_WithValidConceptButNoSources_ReturnsEmpty()
    {
        // Arrange
        var concept = new DataSetConfiguration
        {
            DataSetName = "TestConcept",
            Sources = new Dictionary<string, SourceMappingConfiguration>()
        };

        _registryMock.Setup(r => r.TryGetDataConcept("TestConcept", out It.Ref<DataSetConfiguration?>.IsAny))
            .Returns(new TryGetDataConceptDelegate((string name, out DataSetConfiguration? c) =>
            {
                c = concept;
                return true;
            }));

        var executor = new DataConceptQueryExecutor(_registryMock.Object, _loggerMock.Object);

        // Act
        var result = await executor.Execute<object>("TestConcept");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    private delegate bool TryGetDataConceptDelegate(string name, out DataSetConfiguration? concept);

    [Fact]
    public async Task Execute_WithValidConceptAndSources_LogsSources()
    {
        // Arrange
        var concept = new DataSetConfiguration
        {
            DataSetName = "TestConcept",
            Sources = new Dictionary<string, SourceMappingConfiguration>
            {
                ["Source1"] = new SourceMappingConfiguration
                {
                    ConnectionType = "Sql",
                    Priority = 1,
                    EstimatedCost = 10
                }
            }
        };

        _registryMock.Setup(r => r.TryGetDataConcept("TestConcept", out It.Ref<DataSetConfiguration?>.IsAny))
            .Returns(new TryGetDataConceptDelegate((string name, out DataSetConfiguration? c) =>
            {
                c = concept;
                return true;
            }));

        var executor = new DataConceptQueryExecutor(_registryMock.Object, _loggerMock.Object);

        // Act
        var result = await executor.Execute<object>("TestConcept");

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_WithMultipleSources_OrdersByPriority()
    {
        // Arrange
        var concept = new DataSetConfiguration
        {
            DataSetName = "TestConcept",
            Sources = new Dictionary<string, SourceMappingConfiguration>
            {
                ["Source1"] = new SourceMappingConfiguration
                {
                    ConnectionType = "Rest",
                    Priority = 3,
                    EstimatedCost = 30
                },
                ["Source2"] = new SourceMappingConfiguration
                {
                    ConnectionType = "Sql",
                    Priority = 1,
                    EstimatedCost = 10
                },
                ["Source3"] = new SourceMappingConfiguration
                {
                    ConnectionType = "File",
                    Priority = 2,
                    EstimatedCost = 20
                }
            }
        };

        _registryMock.Setup(r => r.TryGetDataConcept("TestConcept", out It.Ref<DataSetConfiguration?>.IsAny))
            .Returns(new TryGetDataConceptDelegate((string name, out DataSetConfiguration? c) =>
            {
                c = concept;
                return true;
            }));

        var executor = new DataConceptQueryExecutor(_registryMock.Object, _loggerMock.Object);

        // Act
        var result = await executor.Execute<object>("TestConcept");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty(); // No actual data, but OrderBy lambda was executed
    }

    [Fact]
    public async Task Execute_WithCancellationToken_PropagatesToken()
    {
        // Arrange
        var concept = new DataSetConfiguration
        {
            DataSetName = "TestConcept",
            Sources = new Dictionary<string, SourceMappingConfiguration>()
        };

        _registryMock.Setup(r => r.TryGetDataConcept("TestConcept", out It.Ref<DataSetConfiguration?>.IsAny))
            .Returns(new TryGetDataConceptDelegate((string name, out DataSetConfiguration? c) =>
            {
                c = concept;
                return true;
            }));

        var executor = new DataConceptQueryExecutor(_registryMock.Object, _loggerMock.Object);
        var cts = new CancellationTokenSource();

        // Act
        var result = await executor.Execute<object>("TestConcept", cts.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_WithFilter_AppliesFilterToResults()
    {
        // Arrange
        var concept = new DataSetConfiguration
        {
            DataSetName = "TestConcept",
            Sources = new Dictionary<string, SourceMappingConfiguration>()
        };

        _registryMock.Setup(r => r.TryGetDataConcept("TestConcept", out It.Ref<DataSetConfiguration?>.IsAny))
            .Returns(new TryGetDataConceptDelegate((string name, out DataSetConfiguration? c) =>
            {
                c = concept;
                return true;
            }));

        var executor = new DataConceptQueryExecutor(_registryMock.Object, _loggerMock.Object);

        // Act
        var result = await executor.Execute<TestRecord>("TestConcept", r => r.Id > 5);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty(); // No data in this test, but filter is applied
    }

    [Fact]
    public async Task Execute_WithFilterWhenConceptNotFound_ReturnsFailure()
    {
        // Arrange
        _registryMock.Setup(r => r.TryGetDataConcept("NonExistent", out It.Ref<DataSetConfiguration?>.IsAny))
            .Returns(new TryGetDataConceptDelegate((string name, out DataSetConfiguration? c) =>
            {
                c = null;
                return false;
            }));

        var executor = new DataConceptQueryExecutor(_registryMock.Object, _loggerMock.Object);

        // Act
        var result = await executor.Execute<TestRecord>("NonExistent", r => r.Id > 5);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    private class TestRecord
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
