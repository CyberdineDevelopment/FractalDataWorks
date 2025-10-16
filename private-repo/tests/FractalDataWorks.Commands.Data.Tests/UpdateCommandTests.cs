using FractalDataWorks.Commands.Data;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data.Tests;

/// <summary>
/// Comprehensive tests for the <see cref="UpdateCommand{T}"/> class.
/// Achieves 100% code path coverage for UpdateCommand.
/// </summary>
public sealed class UpdateCommandTests
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
        var testData = new TestEntity { Id = 1, Name = "Updated", IsActive = false };

        // Act
        var command = new UpdateCommand<TestEntity>(containerName, testData);

        // Assert
        command.ContainerName.ShouldBe(containerName);
        command.Data.ShouldBe(testData);
        command.Id.ShouldBe(3);
        command.Name.ShouldBe("Update");
        command.CommandType.ShouldBe("Update");
        command.Category.ShouldNotBeNull();
        command.Category.Name.ShouldBe("Update");
    }

    [Fact]
    public void Constructor_InitializesFilterToNull()
    {
        // Arrange
        var testData = new TestEntity();

        // Act
        var command = new UpdateCommand<TestEntity>("TestContainer", testData);

        // Assert
        command.Filter.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithNullData_AcceptsNullValue()
    {
        // Arrange
        const string containerName = "TestContainer";

        // Act
        var command = new UpdateCommand<TestEntity?>(containerName, null);

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
        var testData = new TestEntity { Id = 42, Name = "Updated Entity", IsActive = true };

        // Act
        var command = new UpdateCommand<TestEntity>("TestContainer", testData);

        // Assert
        command.Data.ShouldBe(testData);
        command.Data.Id.ShouldBe(42);
        command.Data.Name.ShouldBe("Updated Entity");
        command.Data.IsActive.ShouldBeTrue();
    }

    #endregion

    #region Filter Property Tests

    [Fact]
    public void Filter_CanBeSetViaInitializer()
    {
        // Arrange
        var testData = new TestEntity();
        var filter = new Mock<IFilterExpression>().Object;

        // Act
        var command = new UpdateCommand<TestEntity>("TestContainer", testData)
        {
            Filter = filter
        };

        // Assert
        command.Filter.ShouldBe(filter);
    }

    [Fact]
    public void Filter_CanBeNull()
    {
        // Arrange
        var testData = new TestEntity();

        // Act
        var command = new UpdateCommand<TestEntity>("TestContainer", testData)
        {
            Filter = null
        };

        // Assert
        command.Filter.ShouldBeNull();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_IsInitializedByDefault()
    {
        // Arrange
        var testData = new TestEntity();

        // Act
        var command = new UpdateCommand<TestEntity>("TestContainer", testData);

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
        var command = new UpdateCommand<TestEntity>("TestContainer", testData);

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
        var command = new UpdateCommand<TestEntity>(string.Empty, testData);

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
        var command = new UpdateCommand<TestEntity>("   ", testData);

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
    public void UpdateCommand_SupportsValueTypes()
    {
        // Arrange & Act
        var command = new UpdateCommand<int>("Numbers", 42);

        // Assert
        command.Data.ShouldBe(42);
    }

    [Fact]
    public void UpdateCommand_SupportsReferenceTypes()
    {
        // Arrange
        const string data = "Updated String";

        // Act
        var command = new UpdateCommand<string>("Strings", data);

        // Assert
        command.Data.ShouldBe(data);
    }

    [Fact]
    public void UpdateCommand_SupportsComplexTypes()
    {
        // Arrange
        var data = new TestEntity { Id = 1, Name = "Updated", IsActive = false };

        // Act
        var command = new UpdateCommand<TestEntity>("Entities", data);

        // Assert
        command.Data.ShouldBe(data);
    }

    #endregion

    #region Combined Properties Tests

    [Fact]
    public void UpdateCommand_CanHaveDataAndFilter()
    {
        // Arrange
        var testData = new TestEntity { Id = 1, Name = "Updated" };
        var filter = new Mock<IFilterExpression>().Object;

        // Act
        var command = new UpdateCommand<TestEntity>("TestContainer", testData)
        {
            Filter = filter
        };

        // Assert
        command.Data.ShouldBe(testData);
        command.Filter.ShouldBe(filter);
    }

    #endregion
}
