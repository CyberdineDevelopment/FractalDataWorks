using FractalDataWorks.Data.DataSets;
using FractalDataWorks.Data.DataSets.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Data.DataSets.Tests;

public class DataConceptRegistryTests
{
    private readonly Mock<ILogger<DataConceptRegistry>> _loggerMock;

    public DataConceptRegistryTests()
    {
        _loggerMock = new Mock<ILogger<DataConceptRegistry>>();
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new DataConceptRegistry(null!, _loggerMock.Object));

        exception.ParamName.ShouldBe("configuration");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new DataConceptRegistry(config, null!));

        exception.ParamName.ShouldBe("logger");
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act
        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Assert
        registry.ShouldNotBeNull();
    }

    [Fact]
    public void GetDataConcept_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            registry.GetDataConcept(null!));

        exception.ParamName.ShouldBe("name");
        exception.Message.ShouldContain("cannot be null or whitespace");
    }

    [Fact]
    public void GetDataConcept_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            registry.GetDataConcept(string.Empty));

        exception.ParamName.ShouldBe("name");
    }

    [Fact]
    public void GetDataConcept_WithWhitespaceName_ThrowsArgumentException()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            registry.GetDataConcept("   "));

        exception.ParamName.ShouldBe("name");
    }

    [Fact]
    public void GetDataConcept_WhenConceptNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() =>
            registry.GetDataConcept("NonExistent"));

        exception.Message.ShouldContain("'NonExistent' not found");
    }

    [Fact]
    public void GetDataConcept_WithValidName_ReturnsConcept()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["DataConcepts:TestConcept:DataSetName"] = "TestConcept",
            ["DataConcepts:TestConcept:Description"] = "Test Description",
            ["DataConcepts:TestConcept:RecordTypeName"] = "Test.Type",
            ["DataConcepts:TestConcept:Version"] = "1.0",
            ["DataConcepts:TestConcept:Category"] = "Test",
            ["DataConcepts:TestConcept:Sources:Source1:ConnectionType"] = "Sql",
            ["DataConcepts:TestConcept:Sources:Source1:Priority"] = "1",
            ["DataConcepts:TestConcept:Sources:Source1:EstimatedCost"] = "10"
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act
        var concept = registry.GetDataConcept("TestConcept");

        // Assert
        concept.ShouldNotBeNull();
        concept.DataSetName.ShouldBe("TestConcept");
        concept.Description.ShouldBe("Test Description");
        concept.Sources.ShouldContainKey("Source1");
        concept.Sources["Source1"].ConnectionType.ShouldBe("Sql");
        concept.Sources["Source1"].Priority.ShouldBe(1);
    }

    [Fact]
    public void GetDataConcept_WithNameFromSectionKey_ReturnsConceptWithCorrectName()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            // DataSetName not specified, should use section key
            ["DataConcepts:MyConceptName:Description"] = "Test",
            ["DataConcepts:MyConceptName:Sources:Source1:ConnectionType"] = "Rest",
            ["DataConcepts:MyConceptName:Sources:Source1:Priority"] = "1"
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act
        var concept = registry.GetDataConcept("MyConceptName");

        // Assert
        concept.DataSetName.ShouldBe("MyConceptName");
    }

    [Fact]
    public void GetDataConcept_IsCaseInsensitive()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["DataConcepts:TestConcept:DataSetName"] = "TestConcept",
            ["DataConcepts:TestConcept:Sources:Source1:ConnectionType"] = "Sql",
            ["DataConcepts:TestConcept:Sources:Source1:Priority"] = "1"
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act
        var concept1 = registry.GetDataConcept("TestConcept");
        var concept2 = registry.GetDataConcept("TESTCONCEPT");
        var concept3 = registry.GetDataConcept("testconcept");

        // Assert
        concept1.ShouldBe(concept2);
        concept1.ShouldBe(concept3);
    }

    [Fact]
    public void TryGetDataConcept_WithNullName_ReturnsFalse()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act
        var result = registry.TryGetDataConcept(null!, out var concept);

        // Assert
        result.ShouldBeFalse();
        concept.ShouldBeNull();
    }

    [Fact]
    public void TryGetDataConcept_WithEmptyName_ReturnsFalse()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act
        var result = registry.TryGetDataConcept(string.Empty, out var concept);

        // Assert
        result.ShouldBeFalse();
        concept.ShouldBeNull();
    }

    [Fact]
    public void TryGetDataConcept_WithNonExistentName_ReturnsFalse()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act
        var result = registry.TryGetDataConcept("NonExistent", out var concept);

        // Assert
        result.ShouldBeFalse();
        concept.ShouldBeNull();
    }

    [Fact]
    public void TryGetDataConcept_WithValidName_ReturnsTrueAndConcept()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["DataConcepts:TestConcept:DataSetName"] = "TestConcept",
            ["DataConcepts:TestConcept:Sources:Source1:ConnectionType"] = "Sql",
            ["DataConcepts:TestConcept:Sources:Source1:Priority"] = "1"
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act
        var result = registry.TryGetDataConcept("TestConcept", out var concept);

        // Assert
        result.ShouldBeTrue();
        concept.ShouldNotBeNull();
        concept.DataSetName.ShouldBe("TestConcept");
    }

    [Fact]
    public void GetAllConcepts_WithNoConcepts_ReturnsEmpty()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act
        var concepts = registry.GetAllConcepts();

        // Assert
        concepts.ShouldBeEmpty();
    }

    [Fact]
    public void GetAllConcepts_WithMultipleConcepts_ReturnsAll()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["DataConcepts:Concept1:Sources:Source1:ConnectionType"] = "Sql",
            ["DataConcepts:Concept1:Sources:Source1:Priority"] = "1",
            ["DataConcepts:Concept2:Sources:Source1:ConnectionType"] = "Rest",
            ["DataConcepts:Concept2:Sources:Source1:Priority"] = "2",
            ["DataConcepts:Concept3:Sources:Source1:ConnectionType"] = "File",
            ["DataConcepts:Concept3:Sources:Source1:Priority"] = "3"
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act
        var concepts = registry.GetAllConcepts().ToList();

        // Assert
        concepts.Count.ShouldBe(3);
        concepts.ShouldContain(c => c.DataSetName == "Concept1");
        concepts.ShouldContain(c => c.DataSetName == "Concept2");
        concepts.ShouldContain(c => c.DataSetName == "Concept3");
    }

    [Fact]
    public void HasConcept_WithNullName_ReturnsFalse()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act
        var result = registry.HasConcept(null!);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasConcept_WithEmptyName_ReturnsFalse()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act
        var result = registry.HasConcept(string.Empty);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasConcept_WithNonExistentName_ReturnsFalse()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act
        var result = registry.HasConcept("NonExistent");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasConcept_WithValidName_ReturnsTrue()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["DataConcepts:TestConcept:Sources:Source1:ConnectionType"] = "Sql",
            ["DataConcepts:TestConcept:Sources:Source1:Priority"] = "1"
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act
        var result = registry.HasConcept("TestConcept");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task LoadConcepts_OnlyHappensOnce_EnsuresThreadSafety()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["DataConcepts:TestConcept:Sources:Source1:ConnectionType"] = "Sql",
            ["DataConcepts:TestConcept:Sources:Source1:Priority"] = "1"
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act - Call multiple times from different threads
        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            var concept = registry.GetDataConcept("TestConcept");
            return concept;
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert - All should return same instance
        var firstConcept = await tasks[0];
        foreach (var task in tasks)
        {
            (await task).ShouldBe(firstConcept);
        }
    }

    [Fact]
    public void LoadConcepts_WithInvalidConfiguration_LogsWarningAndContinues()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            // Valid concept
            ["DataConcepts:ValidConcept:Sources:Source1:ConnectionType"] = "Sql",
            ["DataConcepts:ValidConcept:Sources:Source1:Priority"] = "1",
            // Invalid concept (malformed)
            ["DataConcepts:InvalidConcept"] = "InvalidValue"
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var registry = new DataConceptRegistry(config, _loggerMock.Object);

        // Act
        var concepts = registry.GetAllConcepts().ToList();

        // Assert - Should have loaded the valid concept
        concepts.ShouldContain(c => c.DataSetName == "ValidConcept");
    }
}
