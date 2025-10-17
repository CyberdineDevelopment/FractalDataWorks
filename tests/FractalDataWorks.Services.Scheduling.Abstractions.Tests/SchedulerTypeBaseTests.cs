using FractalDataWorks.Abstractions;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.Scheduling.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Tests;

/// <summary>
/// Tests for SchedulerTypeBase abstract class.
/// </summary>
public class SchedulerTypeBaseTests
{
    /// <summary>
    /// Test scheduler type for testing SchedulerTypeBase.
    /// </summary>
    private class TestSchedulerType : SchedulerTypeBase<IGenericSchedulingService, IServiceFactory<IGenericSchedulingService, ISchedulingConfiguration>, ISchedulingConfiguration>
    {
        public TestSchedulerType(
            int id,
            string name,
            string schedulingEngine,
            Type jobExecutorType,
            Type triggerType,
            bool supportsRecurring,
            bool supportsDelayed,
            string? category = null)
            : base(id, name, schedulingEngine, jobExecutorType, triggerType, supportsRecurring, supportsDelayed, category)
        {
        }

        public override void Configure(IConfiguration configuration)
        {
            // Test implementation - no action needed
        }

        public override void Register(IServiceCollection services)
        {
            // Test implementation - no action needed
        }
    }

    /// <summary>
    /// Dummy types for testing.
    /// </summary>
    private class DummyJobExecutor { }
    private class DummyTrigger { }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        const int id = 1;
        const string name = "TestScheduler";
        const string engine = "TestEngine";
        var jobExecutorType = typeof(DummyJobExecutor);
        var triggerType = typeof(DummyTrigger);

        // Act
        var scheduler = new TestSchedulerType(id, name, engine, jobExecutorType, triggerType, true, true);

        // Assert
        scheduler.ShouldNotBeNull();
        scheduler.Id.ShouldBe(id);
        scheduler.Name.ShouldBe(name);
        scheduler.SchedulingEngine.ShouldBe(engine);
        scheduler.JobExecutorType.ShouldBe(jobExecutorType);
        scheduler.TriggerType.ShouldBe(triggerType);
        scheduler.SupportsRecurring.ShouldBeTrue();
        scheduler.SupportsDelayed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithNullSchedulingEngine_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new TestSchedulerType(1, "Test", null!, typeof(DummyJobExecutor), typeof(DummyTrigger), true, true))
            .ParamName.ShouldBe("schedulingEngine");
    }

    [Fact]
    public void Constructor_WithNullJobExecutorType_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new TestSchedulerType(1, "Test", "Engine", null!, typeof(DummyTrigger), true, true))
            .ParamName.ShouldBe("jobExecutorType");
    }

    [Fact]
    public void Constructor_WithNullTriggerType_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new TestSchedulerType(1, "Test", "Engine", typeof(DummyJobExecutor), null!, true, true))
            .ParamName.ShouldBe("triggerType");
    }

    [Fact]
    public void Constructor_WithCustomCategory_SetsCategory()
    {
        // Arrange
        const string customCategory = "CustomScheduling";

        // Act
        var scheduler = new TestSchedulerType(1, "Test", "Engine", typeof(DummyJobExecutor), typeof(DummyTrigger), true, true, customCategory);

        // Assert
        scheduler.Category.ShouldBe(customCategory);
    }

    [Fact]
    public void Constructor_WithoutCategory_UsesDefaultSchedulingCategory()
    {
        // Act
        var scheduler = new TestSchedulerType(1, "Test", "Engine", typeof(DummyJobExecutor), typeof(DummyTrigger), true, true);

        // Assert
        scheduler.Category.ShouldBe("Scheduling");
    }

    #endregion

    #region Property Tests

    [Fact]
    public void SchedulingEngine_ReturnsValueSetInConstructor()
    {
        // Arrange
        const string engine = "Quartz.NET";
        var scheduler = new TestSchedulerType(1, "Test", engine, typeof(DummyJobExecutor), typeof(DummyTrigger), true, true);

        // Act & Assert
        scheduler.SchedulingEngine.ShouldBe(engine);
    }

    [Fact]
    public void JobExecutorType_ReturnsValueSetInConstructor()
    {
        // Arrange
        var expectedType = typeof(DummyJobExecutor);
        var scheduler = new TestSchedulerType(1, "Test", "Engine", expectedType, typeof(DummyTrigger), true, true);

        // Act & Assert
        scheduler.JobExecutorType.ShouldBe(expectedType);
    }

    [Fact]
    public void TriggerType_ReturnsValueSetInConstructor()
    {
        // Arrange
        var expectedType = typeof(DummyTrigger);
        var scheduler = new TestSchedulerType(1, "Test", "Engine", typeof(DummyJobExecutor), expectedType, true, true);

        // Act & Assert
        scheduler.TriggerType.ShouldBe(expectedType);
    }

    [Fact]
    public void SupportsRecurring_ReturnsValueSetInConstructor()
    {
        // Arrange
        var scheduler = new TestSchedulerType(1, "Test", "Engine", typeof(DummyJobExecutor), typeof(DummyTrigger), true, false);

        // Act & Assert
        scheduler.SupportsRecurring.ShouldBeTrue();
        scheduler.SupportsDelayed.ShouldBeFalse();
    }

    [Fact]
    public void SupportsDelayed_ReturnsValueSetInConstructor()
    {
        // Arrange
        var scheduler = new TestSchedulerType(1, "Test", "Engine", typeof(DummyJobExecutor), typeof(DummyTrigger), false, true);

        // Act & Assert
        scheduler.SupportsRecurring.ShouldBeFalse();
        scheduler.SupportsDelayed.ShouldBeTrue();
    }

    #endregion

    #region Virtual Property Tests

    [Fact]
    public void SupportsCronExpressions_DefaultsToTrue()
    {
        // Arrange
        var scheduler = new TestSchedulerType(1, "Test", "Engine", typeof(DummyJobExecutor), typeof(DummyTrigger), true, true);

        // Act & Assert
        scheduler.SupportsCronExpressions.ShouldBeTrue();
    }

    [Fact]
    public void SupportsIntervalScheduling_DefaultsToTrue()
    {
        // Arrange
        var scheduler = new TestSchedulerType(1, "Test", "Engine", typeof(DummyJobExecutor), typeof(DummyTrigger), true, true);

        // Act & Assert
        scheduler.SupportsIntervalScheduling.ShouldBeTrue();
    }

    [Fact]
    public void SupportsJobPersistence_DefaultsToFalse()
    {
        // Arrange
        var scheduler = new TestSchedulerType(1, "Test", "Engine", typeof(DummyJobExecutor), typeof(DummyTrigger), true, true);

        // Act & Assert
        scheduler.SupportsJobPersistence.ShouldBeFalse();
    }

    [Fact]
    public void SupportsClustering_DefaultsToFalse()
    {
        // Arrange
        var scheduler = new TestSchedulerType(1, "Test", "Engine", typeof(DummyJobExecutor), typeof(DummyTrigger), true, true);

        // Act & Assert
        scheduler.SupportsClustering.ShouldBeFalse();
    }

    [Fact]
    public void SupportsJobQueuing_DefaultsToFalse()
    {
        // Arrange
        var scheduler = new TestSchedulerType(1, "Test", "Engine", typeof(DummyJobExecutor), typeof(DummyTrigger), true, true);

        // Act & Assert
        scheduler.SupportsJobQueuing.ShouldBeFalse();
    }

    [Fact]
    public void MaxConcurrentJobs_DefaultsToNegativeOne()
    {
        // Arrange
        var scheduler = new TestSchedulerType(1, "Test", "Engine", typeof(DummyJobExecutor), typeof(DummyTrigger), true, true);

        // Act & Assert
        scheduler.MaxConcurrentJobs.ShouldBe(-1);
    }

    #endregion

    #region ServiceTypeBase Integration Tests

    [Fact]
    public void SectionName_IsCorrectlyFormatted()
    {
        // Arrange
        const string name = "QuartzScheduler";
        var scheduler = new TestSchedulerType(1, name, "Quartz.NET", typeof(DummyJobExecutor), typeof(DummyTrigger), true, true);

        // Act & Assert
        scheduler.SectionName.ShouldBe($"Services:Scheduling:{name}");
    }

    [Fact]
    public void DisplayName_IsCorrectlyFormatted()
    {
        // Arrange
        const string name = "QuartzScheduler";
        var scheduler = new TestSchedulerType(1, name, "Quartz.NET", typeof(DummyJobExecutor), typeof(DummyTrigger), true, true);

        // Act & Assert
        scheduler.DisplayName.ShouldBe($"{name} Scheduling Service");
    }

    [Fact]
    public void Description_IncludesSchedulingEngine()
    {
        // Arrange
        const string name = "QuartzScheduler";
        const string engine = "Quartz.NET";
        var scheduler = new TestSchedulerType(1, name, engine, typeof(DummyJobExecutor), typeof(DummyTrigger), true, true);

        // Act & Assert
        scheduler.Description.ShouldBe($"Scheduling service using {engine} engine");
    }

    #endregion
}
