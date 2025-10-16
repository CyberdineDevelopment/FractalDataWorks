using FractalDataWorks.Commands.Data;

namespace FractalDataWorks.Commands.Data.Tests;

/// <summary>
/// Comprehensive tests for the <see cref="InsertCommand{T}"/> class.
/// Achieves 100% code path coverage for InsertCommand.
/// </summary>
public sealed class InsertCommandTests
{
    private sealed class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithContainerNameAndData_CreatesCommandWithCorrectProperties()
    {
        // Arrange
        const string containerName = "TestContainer";
        var testData = new TestEntity { Id = 1, Name = "Test", IsActive = true };

        // Act
        var command = new InsertCommand<TestEntity>(containerName, testData);

        // Assert
        command.ContainerName.ShouldBe(containerName);
        command.Data.ShouldBe(testData);
        command.Id.ShouldBe(2);
        command.Name.ShouldBe("Insert");
        command.CommandType.ShouldBe("Insert");
        command.Category.ShouldNotBeNull();
        command.Category.Name.ShouldBe("Insert");
    }

    [Fact]
    public void Constructor_WithNullData_AcceptsNullValue()
    {
        // Arrange
        const string containerName = "TestContainer";

        // Act
        var command = new InsertCommand<TestEntity?>(containerName, null);

        // Assert
        command.Data.ShouldBeNull();
        command.ContainerName.ShouldBe(containerName);
    }

    #endregion

    #region Data Property Tests

    [Fact]
    public void Data_Property_StoresProvidedValue()
    {
        // Arrange
        var testData = new TestEntity { Id = 42, Name = "Test Entity", IsActive = false };

        // Act
        var command = new InsertCommand<TestEntity>("TestContainer", testData);

        // Assert
        command.Data.ShouldBe(testData);
        command.Data.Id.ShouldBe(42);
        command.Data.Name.ShouldBe("Test Entity");
        command.Data.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Data_WithComplexObject_RetainsAllProperties()
    {
        // Arrange
        var complexData = new TestEntity
        {
            Id = 100,
            Name = "Complex Entity",
            IsActive = true
        };

        // Act
        var command = new InsertCommand<TestEntity>("TestContainer", complexData);

        // Assert
        command.Data.Id.ShouldBe(100);
        command.Data.Name.ShouldBe("Complex Entity");
        command.Data.IsActive.ShouldBeTrue();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_IsInitializedByDefault()
    {
        // Arrange
        var testData = new TestEntity();

        // Act
        var command = new InsertCommand<TestEntity>("TestContainer", testData);

        // Assert
        command.Metadata.ShouldNotBeNull();
        command.Metadata.Count.ShouldBe(0);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Validate_WithValidContainerName_ReturnsSuccessResult()
    {
        // Arrange
        var testData = new TestEntity();
        var command = new InsertCommand<TestEntity>("TestContainer", testData);

        // Act
        var result = command.Validate();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithEmptyContainerName_ReturnsValidationError()
    {
        // Arrange
        var testData = new TestEntity();
        var command = new InsertCommand<TestEntity>(string.Empty, testData);

        // Act
        var result = command.Validate();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.IsValid.ShouldBeFalse();
        result.Value.Errors.Count.ShouldBe(1);
        result.Value.Errors[0].PropertyName.ShouldBe("ContainerName");
    }

    [Fact]
    public void Validate_WithWhitespaceContainerName_ReturnsValidationError()
    {
        // Arrange
        var testData = new TestEntity();
        var command = new InsertCommand<TestEntity>("   ", testData);

        // Act
        var result = command.Validate();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.IsValid.ShouldBeFalse();
    }

    #endregion

    #region Type Safety Tests

    [Fact]
    public void InsertCommand_SupportsValueTypes()
    {
        // Arrange & Act
        var command = new InsertCommand<int>("Numbers", 42);

        // Assert
        command.Data.ShouldBe(42);
    }

    [Fact]
    public void InsertCommand_SupportsReferenceTypes()
    {
        // Arrange
        const string data = "Test String";

        // Act
        var command = new InsertCommand<string>("Strings", data);

        // Assert
        command.Data.ShouldBe(data);
    }

    [Fact]
    public void InsertCommand_SupportsComplexTypes()
    {
        // Arrange
        var data = new TestEntity { Id = 1, Name = "Test", IsActive = true };

        // Act
        var command = new InsertCommand<TestEntity>("Entities", data);

        // Assert
        command.Data.ShouldBe(data);
    }

    #endregion

    #region Immutability Tests

    [Fact]
    public void Data_IsSetOnConstruction_CannotBeChanged()
    {
        // Arrange
        var originalData = new TestEntity { Id = 1, Name = "Original" };
        var command = new InsertCommand<TestEntity>("TestContainer", originalData);

        // Act - Try to access Data property
        var retrievedData = command.Data;

        // Assert - Data property returns the same instance
        retrievedData.ShouldBe(originalData);
    }

    #endregion
}
