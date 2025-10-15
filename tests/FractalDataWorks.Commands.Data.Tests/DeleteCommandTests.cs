using FractalDataWorks.Commands.Data;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data.Tests;

/// <summary>
/// Comprehensive tests for the <see cref="DeleteCommand"/> class.
/// Achieves 100% code path coverage for DeleteCommand.
/// </summary>
public sealed class DeleteCommandTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithContainerName_CreatesCommandWithCorrectProperties()
    {
        // Arrange
        const string containerName = "TestContainer";

        // Act
        var command = new DeleteCommand(containerName);

        // Assert
        command.ContainerName.ShouldBe(containerName);
        command.Id.ShouldBe(4);
        command.Name.ShouldBe("Delete");
        command.CommandType.ShouldBe("Delete");
        command.Category.ShouldNotBeNull();
        command.Category.Name.ShouldBe("Delete");
    }

    [Fact]
    public void Constructor_InitializesFilterToNull()
    {
        // Arrange
        const string containerName = "TestContainer";

        // Act
        var command = new DeleteCommand(containerName);

        // Assert
        command.Filter.ShouldBeNull();
    }

    #endregion

    #region Filter Property Tests

    [Fact]
    public void Filter_CanBeSetViaInitializer()
    {
        // Arrange
        var filter = new Mock<IFilterExpression>().Object;

        // Act
        var command = new DeleteCommand("TestContainer")
        {
            Filter = filter
        };

        // Assert
        command.Filter.ShouldBe(filter);
    }

    [Fact]
    public void Filter_CanBeNull()
    {
        // Act
        var command = new DeleteCommand("TestContainer")
        {
            Filter = null
        };

        // Assert
        command.Filter.ShouldBeNull();
    }

    [Fact]
    public void Filter_CanBeSetToComplexExpression()
    {
        // Arrange
        var mockFilter = new Mock<IFilterExpression>();
        mockFilter.Setup(f => f.LogicalOperator).Returns((LogicalOperator?)LogicalOperator.And);

        // Act
        var command = new DeleteCommand("TestContainer")
        {
            Filter = mockFilter.Object
        };

        // Assert
        command.Filter.ShouldNotBeNull();
        command.Filter.LogicalOperator.ShouldBe(LogicalOperator.And);
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_IsInitializedByDefault()
    {
        // Act
        var command = new DeleteCommand("TestContainer");

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
        var command = new DeleteCommand("TestContainer");

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
        var command = new DeleteCommand(string.Empty);

        // Act
        var result = command.Validate();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.IsValid.ShouldBeFalse();
        result.Value.Errors.Count.ShouldBe(1);
        result.Value.Errors[0].PropertyName.ShouldBe("ContainerName");
        result.Value.Errors[0].ErrorMessage.ShouldBe("Container name is required");
    }

    [Fact]
    public void Validate_WithWhitespaceContainerName_ReturnsValidationError()
    {
        // Arrange
        var command = new DeleteCommand("   ");

        // Act
        var result = command.Validate();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_WithNullContainerName_ReturnsValidationError()
    {
        // Arrange
        var command = new DeleteCommand(null!);

        // Act
        var result = command.Validate();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.IsValid.ShouldBeFalse();
    }

    #endregion

    #region Category Tests

    [Fact]
    public void Category_IsDeleteCategory()
    {
        // Arrange
        var command = new DeleteCommand("TestContainer");

        // Act
        var category = command.Category;

        // Assert
        category.ShouldNotBeNull();
        category.Name.ShouldBe("Delete");
        category.IsMutation.ShouldBeTrue();
        category.RequiresTransaction.ShouldBeTrue();
    }

    #endregion

    #region Usage Scenarios Tests

    [Fact]
    public void DeleteCommand_WithFilter_CanBeUsedForConditionalDelete()
    {
        // Arrange
        var filter = new Mock<IFilterExpression>().Object;

        // Act
        var command = new DeleteCommand("Customers")
        {
            Filter = filter
        };

        // Assert
        command.ContainerName.ShouldBe("Customers");
        command.Filter.ShouldNotBeNull();
    }

    [Fact]
    public void DeleteCommand_WithoutFilter_CanBeUsedForDeleteAll()
    {
        // Act
        var command = new DeleteCommand("TempData");

        // Assert
        command.ContainerName.ShouldBe("TempData");
        command.Filter.ShouldBeNull();
    }

    #endregion

    #region Non-Generic Command Tests

    [Fact]
    public void DeleteCommand_IsNonGeneric_UnlikeOtherCommands()
    {
        // Arrange & Act
        var command = new DeleteCommand("TestContainer");

        // Assert - DeleteCommand should not have Data property
        command.ShouldBeOfType<DeleteCommand>();
        typeof(DeleteCommand).GetProperty("Data").ShouldBeNull();
    }

    #endregion
}
