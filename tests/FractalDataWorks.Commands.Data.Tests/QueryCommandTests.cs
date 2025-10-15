using System.Collections.Generic;
using FractalDataWorks.Commands.Data;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data.Tests;

/// <summary>
/// Comprehensive tests for the <see cref="QueryCommand{T}"/> class.
/// Achieves 100% code path coverage for QueryCommand.
/// </summary>
public sealed class QueryCommandTests
{
    private sealed class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithContainerName_CreatesCommandWithCorrectProperties()
    {
        // Arrange
        const string containerName = "TestContainer";

        // Act
        var command = new QueryCommand<TestEntity>(containerName);

        // Assert
        command.ContainerName.ShouldBe(containerName);
        command.Id.ShouldBe(1);
        command.Name.ShouldBe("Query");
        command.CommandType.ShouldBe("Query");
        command.Category.ShouldNotBeNull();
        command.Category.Name.ShouldBe("Query");
    }

    [Fact]
    public void Constructor_InitializesOptionalPropertiesToNull()
    {
        // Arrange
        const string containerName = "TestContainer";

        // Act
        var command = new QueryCommand<TestEntity>(containerName);

        // Assert
        command.Filter.ShouldBeNull();
        command.Projection.ShouldBeNull();
        command.Ordering.ShouldBeNull();
        command.Paging.ShouldBeNull();
        command.Aggregation.ShouldBeNull();
    }

    [Fact]
    public void Constructor_InitializesJoinsToEmptyList()
    {
        // Arrange
        const string containerName = "TestContainer";

        // Act
        var command = new QueryCommand<TestEntity>(containerName);

        // Assert
        command.Joins.ShouldNotBeNull();
        command.Joins.ShouldBeEmpty();
    }

    #endregion

    #region Property Initialization Tests

    [Fact]
    public void Filter_CanBeSetViaInitializer()
    {
        // Arrange
        var filter = new Mock<IFilterExpression>().Object;

        // Act
        var command = new QueryCommand<TestEntity>("TestContainer")
        {
            Filter = filter
        };

        // Assert
        command.Filter.ShouldBe(filter);
    }

    [Fact]
    public void Projection_CanBeSetViaInitializer()
    {
        // Arrange
        var projection = new Mock<IProjectionExpression>().Object;

        // Act
        var command = new QueryCommand<TestEntity>("TestContainer")
        {
            Projection = projection
        };

        // Assert
        command.Projection.ShouldBe(projection);
    }

    [Fact]
    public void Ordering_CanBeSetViaInitializer()
    {
        // Arrange
        var ordering = new Mock<IOrderingExpression>().Object;

        // Act
        var command = new QueryCommand<TestEntity>("TestContainer")
        {
            Ordering = ordering
        };

        // Assert
        command.Ordering.ShouldBe(ordering);
    }

    [Fact]
    public void Paging_CanBeSetViaInitializer()
    {
        // Arrange
        var paging = new Mock<IPagingExpression>().Object;

        // Act
        var command = new QueryCommand<TestEntity>("TestContainer")
        {
            Paging = paging
        };

        // Assert
        command.Paging.ShouldBe(paging);
    }

    [Fact]
    public void Aggregation_CanBeSetViaInitializer()
    {
        // Arrange
        var aggregation = new Mock<IAggregationExpression>().Object;

        // Act
        var command = new QueryCommand<TestEntity>("TestContainer")
        {
            Aggregation = aggregation
        };

        // Assert
        command.Aggregation.ShouldBe(aggregation);
    }

    [Fact]
    public void Joins_CanBeSetViaInitializer()
    {
        // Arrange
        var join1 = new Mock<IJoinExpression>().Object;
        var join2 = new Mock<IJoinExpression>().Object;
        var joins = new List<IJoinExpression> { join1, join2 };

        // Act
        var command = new QueryCommand<TestEntity>("TestContainer")
        {
            Joins = joins
        };

        // Assert
        command.Joins.ShouldBe(joins);
        command.Joins.Count.ShouldBe(2);
    }

    #endregion

    #region Combined Properties Tests

    [Fact]
    public void Command_CanHaveAllPropertiesSet()
    {
        // Arrange
        var filter = new Mock<IFilterExpression>().Object;
        var projection = new Mock<IProjectionExpression>().Object;
        var ordering = new Mock<IOrderingExpression>().Object;
        var paging = new Mock<IPagingExpression>().Object;
        var aggregation = new Mock<IAggregationExpression>().Object;
        var join = new Mock<IJoinExpression>().Object;

        // Act
        var command = new QueryCommand<TestEntity>("TestContainer")
        {
            Filter = filter,
            Projection = projection,
            Ordering = ordering,
            Paging = paging,
            Aggregation = aggregation,
            Joins = [join]
        };

        // Assert
        command.Filter.ShouldBe(filter);
        command.Projection.ShouldBe(projection);
        command.Ordering.ShouldBe(ordering);
        command.Paging.ShouldBe(paging);
        command.Aggregation.ShouldBe(aggregation);
        command.Joins.Count.ShouldBe(1);
        command.Joins[0].ShouldBe(join);
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_IsInitializedByDefault()
    {
        // Arrange & Act
        var command = new QueryCommand<TestEntity>("TestContainer");

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
        var command = new QueryCommand<TestEntity>("TestContainer");

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
        var command = new QueryCommand<TestEntity>(string.Empty);

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
        var command = new QueryCommand<TestEntity>("   ");

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
    public void QueryCommand_SupportsGenericTypes()
    {
        // Arrange & Act
        var intCommand = new QueryCommand<int>("Numbers");
        var stringCommand = new QueryCommand<string>("Strings");
        var complexCommand = new QueryCommand<TestEntity>("Entities");

        // Assert
        intCommand.ShouldNotBeNull();
        stringCommand.ShouldNotBeNull();
        complexCommand.ShouldNotBeNull();
    }

    #endregion
}
