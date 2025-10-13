using FractalDataWorks.Services.Scheduling.Abstractions.EnhancedEnums.TriggerTypeImplementations;
using Moq;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Tests.TriggerTypes;

/// <summary>
/// Tests for Manual trigger type.
/// </summary>
public class ManualTriggerTests
{
    private readonly Manual _manual;

    public ManualTriggerTests()
    {
        _manual = new Manual();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Assert
        _manual.Id.ShouldBe(4);
        _manual.Name.ShouldBe("Manual");
        _manual.RequiresSchedule.ShouldBeFalse();
        _manual.IsImmediate.ShouldBeTrue();
    }

    #endregion

    #region CalculateNextExecution Tests

    [Fact]
    public void CalculateNextExecution_AlwaysReturnsNull()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>());

        // Act
        var result = _manual.CalculateNextExecution(mockTrigger.Object, null);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void CalculateNextExecution_WithLastExecution_StillReturnsNull()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>());
        var lastExecution = DateTime.UtcNow.AddHours(-1);

        // Act
        var result = _manual.CalculateNextExecution(mockTrigger.Object, lastExecution);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void CalculateNextExecution_WithNullTrigger_ReturnsNull()
    {
        // Act
        var result = _manual.CalculateNextExecution(null!, null);

        // Assert
        result.ShouldBeNull();
    }

    #endregion

    #region ValidateTrigger Tests

    [Fact]
    public void ValidateTrigger_WithNullTrigger_ReturnsFailure()
    {
        // Act
        var result = _manual.ValidateTrigger(null!);

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
        var result = _manual.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void ValidateTrigger_WithEmptyConfiguration_ReturnsSuccess()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>());

        // Act
        var result = _manual.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTrigger_WithDescription_ReturnsSuccess()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Manual.DescriptionKey, "Test description" }
        });

        // Act
        var result = _manual.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTrigger_WithInvalidDescription_ReturnsFailure()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Manual.DescriptionKey, 123 } // Not a string
        });

        // Act
        var result = _manual.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Description");
    }

    [Fact]
    public void ValidateTrigger_WithRequiredRole_ReturnsSuccess()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Manual.RequiredRoleKey, "Admin" }
        });

        // Act
        var result = _manual.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTrigger_WithInvalidRequiredRole_ReturnsFailure()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Manual.RequiredRoleKey, 456 } // Not a string
        });

        // Act
        var result = _manual.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("RequiredRole");
    }

    [Fact]
    public void ValidateTrigger_WithAllowConcurrent_ReturnsSuccess()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Manual.AllowConcurrentKey, true }
        });

        // Act
        var result = _manual.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTrigger_WithInvalidAllowConcurrent_ReturnsFailure()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Manual.AllowConcurrentKey, "not a bool" }
        });

        // Act
        var result = _manual.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("AllowConcurrent");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData(0)]
    [InlineData(1)]
    public void ValidateTrigger_WithVariousAllowConcurrentValues_ReturnsSuccess(object value)
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Manual.AllowConcurrentKey, value }
        });

        // Act
        var result = _manual.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTrigger_WithAllConfigurationOptions_ReturnsSuccess()
    {
        // Arrange
        var mockTrigger = new Mock<IGenericTrigger>();
        mockTrigger.Setup(t => t.Configuration).Returns(new Dictionary<string, object>
        {
            { Manual.DescriptionKey, "Test trigger" },
            { Manual.RequiredRoleKey, "Admin" },
            { Manual.AllowConcurrentKey, false }
        });

        // Act
        var result = _manual.ValidateTrigger(mockTrigger.Object);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    #endregion
}
