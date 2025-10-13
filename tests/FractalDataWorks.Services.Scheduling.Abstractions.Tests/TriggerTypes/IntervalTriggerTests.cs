using FractalDataWorks.Services.Scheduling.Abstractions.EnhancedEnums.TriggerTypeImplementations;
using Moq;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Tests.TriggerTypes;

/// <summary>
/// Tests for Interval trigger type.
/// </summary>
public class IntervalTriggerTests
{
    private readonly Interval _interval;

    public IntervalTriggerTests()
    {
        _interval = new Interval();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Assert
        _interval.Id.ShouldBe(2);
        _interval.Name.ShouldBe("Interval");
        _interval.RequiresSchedule.ShouldBeTrue();
        _interval.IsImmediate.ShouldBeFalse();
    }

    #endregion

    #region CalculateNextExecution Tests

    [Fact]
    public void CalculateNextExecution_WithNullTrigger_ReturnsNull()
    {
        // Act
        var result = _interval.CalculateNextExecution(null!, null);

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
        var result = _interval.CalculateNextExecution(mockTrigger.Object, null);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void CalculateNextExecution_WithMissingInterval_ReturnsNull()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>());

        // Act
        var result = _interval.CalculateNextExecution(mockTrigger.Object, null);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void CalculateNextExecution_WithNegativeInterval_ReturnsNull()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Interval.IntervalMinutesKey, -10 }
        });

        // Act
        var result = _interval.CalculateNextExecution(mockTrigger.Object, null);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void CalculateNextExecution_WithValidInterval_ReturnsNextExecution()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Interval.IntervalMinutesKey, 30 }
        });

        // Act
        var result = _interval.CalculateNextExecution(mockTrigger.Object, null);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeGreaterThan(DateTime.UtcNow);
    }

    [Fact]
    public void CalculateNextExecution_WithLastExecution_AddsIntervalToLastExecution()
    {
        // Arrange
        var lastExecution = DateTime.UtcNow.AddHours(-1);
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Interval.IntervalMinutesKey, 30 }
        });

        // Act
        var result = _interval.CalculateNextExecution(mockTrigger.Object, lastExecution);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(lastExecution.AddMinutes(30), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CalculateNextExecution_WithStartTime_UsesStartTime()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(1);
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Interval.IntervalMinutesKey, 30 },
            { Interval.StartTimeKey, startTime }
        });

        // Act
        var result = _interval.CalculateNextExecution(mockTrigger.Object, null);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(startTime.AddMinutes(30), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CalculateNextExecution_WithTimezone_CalculatesCorrectly()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Interval.IntervalMinutesKey, 60 },
            { Interval.TimeZoneIdKey, "America/New_York" }
        });

        // Act
        var result = _interval.CalculateNextExecution(mockTrigger.Object, null);

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
            { Interval.IntervalMinutesKey, 30 },
            { Interval.TimeZoneIdKey, "Invalid/Timezone" }
        });

        // Act
        var result = _interval.CalculateNextExecution(mockTrigger.Object, null);

        // Assert
        result.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(60)]
    [InlineData(1440)]
    [InlineData("30")]
    [InlineData(30.0)]
    public void CalculateNextExecution_WithVariousIntervalTypes_HandlesCorrectly(object intervalValue)
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Interval.IntervalMinutesKey, intervalValue }
        });

        // Act
        var result = _interval.CalculateNextExecution(mockTrigger.Object, null);

        // Assert
        result.ShouldNotBeNull();
    }

    #endregion

    #region ValidateTrigger Tests

    [Fact]
    public void ValidateTrigger_WithNullTrigger_ReturnsFailure()
    {
        // Act
        var result = _interval.ValidateTrigger(null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void ValidateTrigger_WithNullConfiguration_ReturnsFailure()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns((Dictionary<string, object>)null!);

        // Act
        var result = _interval.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void ValidateTrigger_WithMissingInterval_ReturnsFailure()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>());

        // Act
        var result = _interval.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.CurrentMessage.ShouldNotBeNullOrEmpty();
        result.CurrentMessage.ShouldContain("Interval in minutes is required");
    }

    [Fact]
    public void ValidateTrigger_WithNegativeInterval_ReturnsFailure()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Interval.IntervalMinutesKey, -10 }
        });

        // Act
        var result = _interval.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.CurrentMessage.ShouldNotBeNullOrEmpty();
        result.CurrentMessage.ShouldContain("must be greater than 0");
    }

    [Fact]
    public void ValidateTrigger_WithZeroInterval_ReturnsFailure()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Interval.IntervalMinutesKey, 0 }
        });

        // Act
        var result = _interval.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void ValidateTrigger_WithValidInterval_ReturnsSuccess()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Interval.IntervalMinutesKey, 30 }
        });

        // Act
        var result = _interval.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTrigger_WithInvalidStartTime_ReturnsFailure()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Interval.IntervalMinutesKey, 30 },
            { Interval.StartTimeKey, "not a date" }
        });

        // Act
        var result = _interval.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.CurrentMessage.ShouldNotBeNullOrEmpty();
        result.CurrentMessage.ShouldContain("Start time must be a valid DateTime");
    }

    [Fact]
    public void ValidateTrigger_WithInvalidTimezone_ReturnsFailure()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Interval.IntervalMinutesKey, 30 },
            { Interval.TimeZoneIdKey, "Invalid/Timezone" }
        });

        // Act
        var result = _interval.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.CurrentMessage.ShouldNotBeNullOrEmpty();
        result.CurrentMessage.ShouldContain("Invalid timezone");
    }

    [Fact]
    public void ValidateTrigger_WithValidStartTimeAndTimezone_ReturnsSuccess()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Interval.IntervalMinutesKey, 30 },
            { Interval.StartTimeKey, DateTime.UtcNow.AddHours(1) },
            { Interval.TimeZoneIdKey, "UTC" }
        });

        // Act
        var result = _interval.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    #endregion
}
