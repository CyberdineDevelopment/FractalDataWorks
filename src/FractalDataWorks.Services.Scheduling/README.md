# FractalDataWorks.Services.Scheduling

## Overview

The Scheduling framework provides ServiceType auto-discovery for job scheduling and task execution with unified interfaces that work across different scheduling providers and execution engines.

## Features

- **ServiceType Auto-Discovery**: Add scheduling packages and they're automatically registered
- **Universal Scheduling Interface**: Same API works with all scheduling providers
- **Dynamic Scheduler Creation**: Scheduling services created via factories
- **Source-Generated Collections**: High-performance scheduler lookup

## Quick Start

### 1. Install Packages

```xml
<ProjectReference Include="..\FractalDataWorks.Services.Scheduling\FractalDataWorks.Services.Scheduling.csproj" />
<ProjectReference Include="..\FractalDataWorks.Services.Scheduling.Quartz\FractalDataWorks.Services.Scheduling.Quartz.csproj" />
```

### 2. Register Services

```csharp
// Program.cs - Zero-configuration registration
builder.Services.AddScoped<IFdwSchedulingProvider, FdwSchedulingProvider>();

// Single line registers ALL discovered scheduling types
SchedulingTypes.Register(builder.Services);
```

### 3. Configure Scheduling

```json
{
  "Scheduling": {
    "QuartzScheduler": {
      "SchedulerType": "Quartz",
      "MaxConcurrentJobs": 10,
      "MisfireThreshold": 60000,
      "EnablePersistence": true,
      "ConnectionString": "Server=localhost;Database=Quartz;Integrated Security=true;"
    }
  }
}
```

### 4. Use Universal Scheduling

```csharp
public class ReportService
{
    private readonly IFdwSchedulingProvider _schedulingProvider;

    public ReportService(IFdwSchedulingProvider schedulingProvider)
    {
        _schedulingProvider = schedulingProvider;
    }

    public async Task<IFdwResult> ScheduleDailyReportAsync()
    {
        var schedulerResult = await _schedulingProvider.GetScheduler("QuartzScheduler");
        if (!schedulerResult.IsSuccess)
            return FdwResult.Failure(schedulerResult.Error);

        using var scheduler = schedulerResult.Value;

        // Universal job scheduling - works with any scheduler
        var job = new ScheduledJob
        {
            Name = "DailyReport",
            Group = "Reports",
            JobType = typeof(DailyReportJob),
            Schedule = CronSchedule.Daily(hour: 6, minute: 0), // 6:00 AM daily
            Data = new Dictionary<string, object>
            {
                ["ReportType"] = "Daily",
                ["Recipients"] = new[] { "admin@company.com" }
            }
        };

        var result = await scheduler.ScheduleJobAsync(job);
        return result;
    }

    public async Task<IFdwResult> ScheduleOneTimeTaskAsync(DateTime executeAt)
    {
        var schedulerResult = await _schedulingProvider.GetScheduler("QuartzScheduler");
        if (!schedulerResult.IsSuccess)
            return FdwResult.Failure(schedulerResult.Error);

        using var scheduler = schedulerResult.Value;

        var job = new ScheduledJob
        {
            Name = $"OneTime_{Guid.NewGuid()}",
            Group = "OneTime",
            JobType = typeof(CustomTaskJob),
            Schedule = SimpleSchedule.At(executeAt),
            Data = new Dictionary<string, object>
            {
                ["TaskId"] = Guid.NewGuid(),
                ["Priority"] = "High"
            }
        };

        return await scheduler.ScheduleJobAsync(job);
    }
}
```

## Available Scheduling Types

| Package | Scheduler Type | Purpose |
|---------|---------------|---------|
| `FractalDataWorks.Services.Scheduling.Quartz` | Quartz | Quartz.NET scheduler integration |
| `FractalDataWorks.Services.Scheduling.Hangfire` | Hangfire | Hangfire background job processor |
| `FractalDataWorks.Services.Scheduling.Timer` | Timer | Simple timer-based scheduling |

## How Auto-Discovery Works

1. **Source Generator Scans**: `[ServiceTypeCollection]` attribute triggers compile-time discovery
2. **Finds Implementations**: Scans referenced assemblies for types inheriting from `SchedulingTypeBase`
3. **Generates Collections**: Creates `SchedulingTypes.All`, `SchedulingTypes.Name()`, etc.
4. **Self-Registration**: Each scheduling type handles its own DI registration

## Adding Custom Scheduling Types

```csharp
// 1. Create your scheduling type (singleton pattern)
public sealed class CustomSchedulingType : SchedulingTypeBase<IFdwScheduler, CustomSchedulingConfiguration, ICustomSchedulingFactory>
{
    public static CustomSchedulingType Instance { get; } = new();

    private CustomSchedulingType() : base(4, "Custom", "Scheduling Providers") { }

    public override Type FactoryType => typeof(ICustomSchedulingFactory);

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<ICustomSchedulingFactory, CustomSchedulingFactory>();
        services.AddScoped<CustomJobStore>();
        services.AddScoped<CustomTriggerBuilder>();
    }
}

// 2. Add package reference - source generator automatically discovers it
// 3. SchedulingTypes.Register(services) will include it automatically
```

## Common Scheduling Patterns

### Recurring Jobs

```csharp
public class MaintenanceService
{
    private readonly IFdwSchedulingProvider _schedulingProvider;

    public MaintenanceService(IFdwSchedulingProvider schedulingProvider)
    {
        _schedulingProvider = schedulingProvider;
    }

    public async Task<IFdwResult> ScheduleMaintenanceJobsAsync()
    {
        var schedulerResult = await _schedulingProvider.GetScheduler("QuartzScheduler");
        if (!schedulerResult.IsSuccess)
            return FdwResult.Failure(schedulerResult.Error);

        using var scheduler = schedulerResult.Value;

        // Database cleanup - weekly on Sunday at 2 AM
        var dbCleanup = new ScheduledJob
        {
            Name = "DatabaseCleanup",
            Group = "Maintenance",
            JobType = typeof(DatabaseCleanupJob),
            Schedule = CronSchedule.Weekly(DayOfWeek.Sunday, hour: 2, minute: 0)
        };

        var dbResult = await scheduler.ScheduleJobAsync(dbCleanup);
        if (!dbResult.IsSuccess)
            return dbResult;

        // Log rotation - daily at midnight
        var logRotation = new ScheduledJob
        {
            Name = "LogRotation",
            Group = "Maintenance",
            JobType = typeof(LogRotationJob),
            Schedule = CronSchedule.Daily(hour: 0, minute: 0)
        };

        return await scheduler.ScheduleJobAsync(logRotation);
    }
}
```

### Job Monitoring and Management

```csharp
public class JobManagementService
{
    private readonly IFdwSchedulingProvider _schedulingProvider;

    public JobManagementService(IFdwSchedulingProvider schedulingProvider)
    {
        _schedulingProvider = schedulingProvider;
    }

    public async Task<IFdwResult<List<JobStatus>>> GetJobStatusAsync()
    {
        var schedulerResult = await _schedulingProvider.GetScheduler("QuartzScheduler");
        if (!schedulerResult.IsSuccess)
            return FdwResult<List<JobStatus>>.Failure(schedulerResult.Error);

        using var scheduler = schedulerResult.Value;

        // Get all scheduled jobs
        var jobsResult = await scheduler.GetAllJobsAsync();
        if (!jobsResult.IsSuccess)
            return FdwResult<List<JobStatus>>.Failure(jobsResult.Error);

        var statuses = new List<JobStatus>();
        foreach (var job in jobsResult.Value)
        {
            var statusResult = await scheduler.GetJobStatusAsync(job.Name, job.Group);
            if (statusResult.IsSuccess)
                statuses.Add(statusResult.Value);
        }

        return FdwResult<List<JobStatus>>.Success(statuses);
    }

    public async Task<IFdwResult> PauseJobAsync(string jobName, string jobGroup)
    {
        var schedulerResult = await _schedulingProvider.GetScheduler("QuartzScheduler");
        if (!schedulerResult.IsSuccess)
            return FdwResult.Failure(schedulerResult.Error);

        using var scheduler = schedulerResult.Value;
        return await scheduler.PauseJobAsync(jobName, jobGroup);
    }

    public async Task<IFdwResult> ResumeJobAsync(string jobName, string jobGroup)
    {
        var schedulerResult = await _schedulingProvider.GetScheduler("QuartzScheduler");
        if (!schedulerResult.IsSuccess)
            return FdwResult.Failure(schedulerResult.Error);

        using var scheduler = schedulerResult.Value;
        return await scheduler.ResumeJobAsync(jobName, jobGroup);
    }
}
```

### Dynamic Job Creation

```csharp
public class DynamicJobService
{
    private readonly IFdwSchedulingProvider _schedulingProvider;

    public DynamicJobService(IFdwSchedulingProvider schedulingProvider)
    {
        _schedulingProvider = schedulingProvider;
    }

    public async Task<IFdwResult> ScheduleDataProcessingJobAsync(int dataSetId, TimeSpan delay)
    {
        var schedulerResult = await _schedulingProvider.GetScheduler("QuartzScheduler");
        if (!schedulerResult.IsSuccess)
            return FdwResult.Failure(schedulerResult.Error);

        using var scheduler = schedulerResult.Value;

        var executeAt = DateTimeOffset.UtcNow.Add(delay);
        var job = new ScheduledJob
        {
            Name = $"DataProcessing_{dataSetId}_{DateTimeOffset.UtcNow.Ticks}",
            Group = "DataProcessing",
            JobType = typeof(DataProcessingJob),
            Schedule = SimpleSchedule.At(executeAt),
            Data = new Dictionary<string, object>
            {
                ["DataSetId"] = dataSetId,
                ["ProcessingMode"] = "Batch",
                ["MaxRetries"] = 3
            }
        };

        return await scheduler.ScheduleJobAsync(job);
    }
}
```

## Architecture Benefits

- **Scheduler Agnostic**: Switch scheduling providers without code changes
- **Zero Configuration**: Add package reference, get functionality
- **Type Safety**: Compile-time validation of scheduling types
- **Performance**: Source-generated collections use FrozenDictionary
- **Scalability**: Each scheduling type manages its own job execution strategy

For complete architecture details, see [Services.Abstractions README](../FractalDataWorks.Services.Abstractions/README.md).