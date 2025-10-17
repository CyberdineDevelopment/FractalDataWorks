using FractalDataWorks.Configuration;
using FractalDataWorks.Configuration.Sources;
using FractalDataWorks.Configuration.Abstractions;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Configuration.Tests;

public class JsonConfigurationSourceTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly Mock<ILogger<JsonConfigurationSource>> _mockLogger;
    private readonly JsonConfigurationSource _source;

    public JsonConfigurationSourceTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), $"JsonConfigTest_{Guid.NewGuid()}");
        _mockLogger = new Mock<ILogger<JsonConfigurationSource>>();
        _source = new JsonConfigurationSource(_mockLogger.Object, _testBasePath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, true);
        }
    }

    private class TestConfig : ConfigurationBase<TestConfig>
    {
        public override string SectionName => "Test";
        public string Value { get; set; } = string.Empty;
    }

    [Fact]
    public void Constructor_WithValidBasePath_CreatesDirectory()
    {
        // Assert
        Directory.Exists(_testBasePath).ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithNullBasePath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new JsonConfigurationSource(_mockLogger.Object, null!));
    }

    [Fact]
    public void IsWritable_ShouldReturnTrue()
    {
        // Assert
        _source.IsWritable.ShouldBeTrue();
    }

    [Fact]
    public void SupportsReload_ShouldReturnFalse()
    {
        // Assert
        _source.SupportsReload.ShouldBeFalse();
    }

    [Fact]
    public async Task Load_WithNoFiles_ReturnsEmptyList()
    {
        // Act
        var result = await _source.Load<TestConfig>();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Save_WithValidConfiguration_SavesFile()
    {
        // Arrange
        var config = new TestConfig
        {
            Id = 1,
            Name = "Test",
            Value = "TestValue"
        };

        // Act
        var result = await _source.Save(config);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var files = Directory.GetFiles(_testBasePath, "TestConfig_*.json");
        files.Length.ShouldBe(1);
    }

    [Fact]
    public async Task Save_ThenLoad_RetrievesConfiguration()
    {
        // Arrange
        var config = new TestConfig
        {
            Id = 1,
            Name = "Test",
            Value = "TestValue"
        };

        // Act
        await _source.Save(config);
        var loadResult = await _source.Load<TestConfig>();

        // Assert
        loadResult.IsSuccess.ShouldBeTrue();
        loadResult.Value.ShouldNotBeNull();
        loadResult.Value.Count().ShouldBe(1);
        var loaded = loadResult.Value.First();
        loaded.Id.ShouldBe(config.Id);
        loaded.Name.ShouldBe(config.Name);
        loaded.Value.ShouldBe(config.Value);
    }

    [Fact]
    public async Task Load_WithId_ReturnsSpecificConfiguration()
    {
        // Arrange
        var config = new TestConfig
        {
            Id = 42,
            Name = "SpecificConfig",
            Value = "SpecificValue"
        };
        await _source.Save(config);

        // Act
        var result = await _source.Load<TestConfig>(42);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(42);
        result.Value.Name.ShouldBe("SpecificConfig");
        result.Value.Value.ShouldBe("SpecificValue");
    }

    [Fact]
    public async Task Load_WithNonExistentId_ReturnsFailure()
    {
        // Act
        var result = await _source.Load<TestConfig>(999);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.CurrentMessage.ShouldNotBeNull();
        result.CurrentMessage.ShouldContain("not found");
    }

    [Fact]
    public async Task Delete_ExistingConfiguration_RemovesFile()
    {
        // Arrange
        var config = new TestConfig { Id = 1, Name = "ToDelete" };
        await _source.Save(config);

        // Act
        var result = await _source.Delete<TestConfig>(1);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var files = Directory.GetFiles(_testBasePath, "TestConfig_*.json");
        files.Length.ShouldBe(0);
    }

    [Fact]
    public async Task Delete_NonExistentConfiguration_ReturnsFailure()
    {
        // Act
        var result = await _source.Delete<TestConfig>(999);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.CurrentMessage.ShouldNotBeNull();
        result.CurrentMessage.ShouldContain("not found");
    }

    [Fact]
    public async Task Load_WithInvalidJson_ContinuesLoadingOtherFiles()
    {
        // Arrange
        var validConfig = new TestConfig { Id = 1, Name = "Valid" };
        await _source.Save(validConfig);

        // Create an invalid JSON file
        var invalidFile = Path.Combine(_testBasePath, "TestConfig_2.json");
        File.WriteAllText(invalidFile, "{ invalid json }");

        // Act
        var result = await _source.Load<TestConfig>();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBe(1); // Only valid config loaded
        result.Value.First().Id.ShouldBe(1);
    }

    [Fact]
    public async Task Load_WithNullDeserializedConfig_SkipsFile()
    {
        // Arrange
        // Create a file that deserializes to null
        var nullFile = Path.Combine(_testBasePath, "TestConfig_0.json");
        File.WriteAllText(nullFile, "null");

        // Act
        var result = await _source.Load<TestConfig>();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Save_MultipleTimes_UpdatesFile()
    {
        // Arrange
        var config = new TestConfig { Id = 1, Name = "Original", Value = "Value1" };
        await _source.Save(config);

        // Act
        config.Value = "Updated";
        await _source.Save(config);

        // Assert
        var loaded = await _source.Load<TestConfig>(1);
        loaded.Value.ShouldNotBeNull();
        loaded.Value.Value.ShouldBe("Updated");
    }

    [Fact]
    public void Name_Property_ReturnsJSON()
    {
        // Assert
        _source.Name.ShouldBe("JSON");
    }
}
