using FractalDataWorks.Services.Scheduling.Abstractions.EnhancedEnums;
using FractalDataWorks.Services.Scheduling.Abstractions.EnhancedEnums.TriggerTypeImplementations;
using Moq;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Tests.TriggerTypes;

/// <summary>
/// Tests for Cron trigger type.
/// </summary>
public class CronTriggerTests
{
    private readonly Cron _cron;

    public CronTriggerTests()
    {
        _cron = new Cron();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Assert
        _cron.Id.ShouldBe(1);
        _cron.Name.ShouldBe("Cron");
        _cron.RequiresSchedule.ShouldBeTrue();
        _cron.IsImmediate.ShouldBeFalse();
    }

    #endregion

    #region CalculateNextExecution Tests

    [Fact]
    public void CalculateNextExecution_WithNullTrigger_ReturnsNull()
    {
        // Act
        var result = _cron.CalculateNextExecution(null!, null);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void CalculateNextExecution_WithNullConfiguration_ReturnsNull()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns((Dictionary<string, object>)null!);

        // Act
        var result = _cron.CalculateNextExecution(mockTrigger.Object, null);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void CalculateNextExecution_WithMissingCronExpression_ReturnsNull()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>());

        // Act
        var result = _cron.CalculateNextExecution(mockTrigger.Object, null);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void CalculateNextExecution_WithInvalidCronExpression_ReturnsNull()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Cron.CronExpressionKey, "invalid cron" }
        });

        // Act
        var result = _cron.CalculateNextExecution(mockTrigger.Object, null);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void CalculateNextExecution_WithValidCronExpression_ReturnsNextOccurrence()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Cron.CronExpressionKey, "0 0 * * *" } // Daily at midnight
        });

        // Act
        var result = _cron.CalculateNextExecution(mockTrigger.Object, null);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeGreaterThan(DateTime.UtcNow);
    }

    [Fact]
    public void CalculateNextExecution_WithTimezone_CalculatesCorrectly()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Cron.CronExpressionKey, "0 9 * * *" }, // Daily at 9 AM
            { Cron.TimeZoneIdKey, "America/New_York" }
        });

        // Act
        var result = _cron.CalculateNextExecution(mockTrigger.Object, null);

        // Assert
        result.ShouldNotBeNull();
        result.Value.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Fact]
    public void CalculateNextExecution_WithInvalidTimezone_FallsBackToUtc()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Cron.CronExpressionKey, "0 9 * * *" },
            { Cron.TimeZoneIdKey, "Invalid/Timezone" }
        });

        // Act
        var result = _cron.CalculateNextExecution(mockTrigger.Object, null);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public void CalculateNextExecution_WithLastExecution_CalculatesFromLastExecution()
    {
        // Arrange
        var lastExecution = DateTime.UtcNow.AddDays(-1);
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Cron.CronExpressionKey, "0 0 * * *" } // Daily at midnight
        });

        // Act
        var result = _cron.CalculateNextExecution(mockTrigger.Object, lastExecution);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeGreaterThan(lastExecution);
    }

    #endregion

    #region ValidateTrigger Tests

    [Fact]
    public void ValidateTrigger_WithNullTrigger_ReturnsFailure()
    {
        // Act
        var result = _cron.ValidateTrigger(null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void ValidateTrigger_WithNullConfiguration_ReturnsFailure()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns((Dictionary<string, object>)null!);

        // Act
        var result = _cron.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void ValidateTrigger_WithMissingCronExpression_ReturnsFailure()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>());

        // Act
        var result = _cron.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Cron expression is required");
    }

    [Fact]
    public void ValidateTrigger_WithInvalidCronExpression_ReturnsFailure()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Cron.CronExpressionKey, "invalid cron" }
        });

        // Act
        var result = _cron.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Invalid cron expression");
    }

    [Fact]
    public void ValidateTrigger_WithValidCronExpression_ReturnsSuccess()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Cron.CronExpressionKey, "0 0 * * *" }
        });

        // Act
        var result = _cron.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTrigger_WithInvalidTimezone_ReturnsFailure()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Cron.CronExpressionKey, "0 0 * * *" },
            { Cron.TimeZoneIdKey, "Invalid/Timezone" }
        });

        // Act
        var result = _cron.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Invalid timezone");
    }

    [Fact]
    public void ValidateTrigger_WithValidTimezone_ReturnsSuccess()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Cron.CronExpressionKey, "0 0 * * *" },
            { Cron.TimeZoneIdKey, "UTC" }
        });

        // Act
        var result = _cron.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTrigger_WithExpiredCronExpression_ReturnsFailure()
    {
        // Arrange - cron expression that will never execute
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Cron.CronExpressionKey, "0 0 31 2 *" } // February 31st (doesn't exist)
        });

        // Act
        var result = _cron.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("will never execute");
    }

    #endregion
}
