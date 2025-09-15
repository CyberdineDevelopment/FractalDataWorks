# FractalDataWorks.Services.Scheduling.Abstractions

Comprehensive scheduling abstractions and interfaces for the FractalDataWorks platform, providing cron-based scheduling capabilities with flexible trigger types and execution management.

## Overview

This project defines the core abstractions for the FractalDataWorks scheduling system, implementing a clean separation between scheduling (WHEN) and execution (HOW). It provides comprehensive scheduling capabilities including cron expressions, timezone handling, trigger management, and execution tracking.

## Key Components

### Core Interfaces

#### `IFractalSchedulingService`
- **Purpose**: Main service interface for scheduling operations that applications use
- **Features**:
  - Create, update, and delete schedules
  - Pause and resume schedule execution
  - Trigger immediate execution
  - Query schedules and execution history
  - Access to underlying scheduler

#### `IFractalScheduler`
- **Purpose**: Core scheduling engine that manages WHEN to trigger process execution
- **Features**:
  - Schedule registration and management
  - Timing and trigger monitoring
  - Bridge to execution handlers
  - Schedule state management (active/paused)

#### `IFractalSchedule`
- **Purpose**: Interface defining a schedule that specifies WHEN and HOW a process should execute
- **Properties**:
  - `ScheduleId`, `ScheduleName` - Identity
  - `ProcessId` - Reference to process for execution
  - `CronExpression` - Timing specification
  - `NextExecution` - Calculated next run time
  - `IsActive` - Schedule status
  - `TimeZoneId` - Timezone for cron calculation
  - `Metadata` - Extensible metadata

#### `IFractalTrigger`
- **Purpose**: Defines when a schedule should trigger execution
- **Features**: Configuration-based trigger definitions with extensible trigger types

### Enhanced Enum Types

#### `TriggerTypeBase`
- **Type**: `abstract class`
- **Purpose**: Base class for all trigger type implementations
- **Properties**: ID, Name, RequiresSchedule, IsImmediate
- **Methods**: `CalculateNextExecution()`, `ValidateTrigger()`

#### Trigger Type Implementations

##### `Cron`
- **Type**: `sealed class` with `[EnumOption("Cron")]`
- **Purpose**: Cron expression-based scheduling with timezone support
- **Features**:
  - Uses Cronos library for cron parsing and calculation
  - Timezone-aware execution with DST handling
  - Supports standard and extended cron formats
  - Validates cron expressions and timezones
  - Configuration keys: `CronExpression`, `TimeZoneId`

##### `Interval`
- **Type**: `sealed class` with `[EnumOption("Interval")]`
- **Purpose**: Fixed interval-based scheduling
- **Features**: Regular interval-based execution (not immediately implemented)

##### `Once`
- **Type**: `sealed class` with `[EnumOption("Once")]`
- **Purpose**: Single execution at specified time
- **Features**: One-time scheduled execution

##### `Manual`
- **Type**: `sealed class` with `[EnumOption("Manual")]`
- **Purpose**: Manual trigger only (no automatic scheduling)
- **Features**: Execution only through explicit triggering

### Model Classes

#### `Schedule`
- **Type**: `sealed class`
- **Interfaces**: `IFractalSchedule`
- **Purpose**: Default implementation of schedule with comprehensive validation and state management
- **Features**:
  - Factory methods (`CreateNew()`, `Create()`)
  - Comprehensive validation logic
  - State management (active status, next execution updates)
  - Metadata support with dictionary backing
  - Cron expression and timezone extraction from triggers
  - Timestamp tracking (created/updated)

#### `Trigger`
- **Type**: Implementation class (referenced but not provided)
- **Interfaces**: `IFractalTrigger`
- **Purpose**: Default trigger implementation with configuration management

### Messages

#### Scheduling Messages (using Message Source Generators)
- `JobCompletedMessage` - Job completion notification
- `JobFailedMessage` - Job failure notification
- `JobScheduledMessage` - Job scheduling notification
- `SchedulingMessage` - Base message type
- `SchedulingMessageCollectionBase` - Message collection

### Service Types

#### `SchedulingServiceType` and `SchedulingServiceTypes`
- **Purpose**: Service type definitions for scheduling implementations
- **Features**: Enhanced enum pattern for service type management

## Dependencies

### Package References
- `Cronos` - Cron expression parsing and calculation
- `Microsoft.Extensions.DependencyInjection.Abstractions` - DI support
- `Microsoft.Extensions.Logging.Abstractions` - Logging contracts

### Project References
- `FractalDataWorks.Services.Execution.Abstractions` - Process execution contracts
- `FractalDataWorks.EnhancedEnums` - Enhanced enum support
- `FractalDataWorks.Messages` - Messaging infrastructure
- `FractalDataWorks.Results` - Result pattern implementation
- `FractalDataWorks.Services` - Base service implementations
- `FractalDataWorks.Services.Abstractions` - Service abstractions
- `FractalDataWorks.EnhancedEnums.SourceGenerators` - Build-time enum generation
- `FractalDataWorks.Messages.SourceGenerators` - Build-time message generation

## Usage Patterns

### Creating and Managing Schedules
```csharp
// Create a daily backup schedule
var cronTrigger = Trigger.CreateCron(
    name: "Daily Backup Trigger",
    cronExpression: "0 2 * * *",  // 2 AM daily
    timeZoneId: "America/New_York"
);

var schedule = Schedule.CreateNew(
    name: "Daily Database Backup",
    processType: "DatabaseBackup",
    processConfiguration: backupConfig,
    trigger: cronTrigger,
    description: "Automated daily backup of production database"
);

// Validate the schedule
var validationResult = schedule.Validate();
if (validationResult.IsSuccess)
{
    // Create the schedule in the service
    await schedulingService.CreateScheduleAsync(schedule, cancellationToken);
}
```

### Using the Scheduling Service
```csharp
// Inject the scheduling service
public class BackupController
{
    private readonly IFractalSchedulingService _schedulingService;
    
    public BackupController(IFractalSchedulingService schedulingService)
    {
        _schedulingService = schedulingService;
    }
    
    public async Task<IActionResult> ManageBackupSchedule()
    {
        // Get existing schedule
        var scheduleResult = await _schedulingService.GetScheduleAsync(
            "daily-backup", 
            cancellationToken
        );
        
        if (scheduleResult.IsSuccess && scheduleResult.Value != null)
        {
            // Pause temporarily
            await _schedulingService.PauseScheduleAsync(
                "daily-backup", 
                cancellationToken
            );
            
            // Trigger immediate execution
            await _schedulingService.TriggerScheduleAsync(
                "daily-backup", 
                cancellationToken
            );
            
            // Resume normal schedule
            await _schedulingService.ResumeScheduleAsync(
                "daily-backup", 
                cancellationToken
            );
        }
        
        return Ok();
    }
}
```

### Cron Trigger Configuration
```csharp
// Various cron expression examples
var dailyTrigger = new Dictionary<string, object>
{
    ["CronExpression"] = "0 9 * * *",           // Daily at 9 AM
    ["TimeZoneId"] = "America/New_York"
};

var weekdayTrigger = new Dictionary<string, object>
{
    ["CronExpression"] = "0 9 * * MON-FRI",     // Weekdays at 9 AM
    ["TimeZoneId"] = "UTC"
};

var hourlyTrigger = new Dictionary<string, object>
{
    ["CronExpression"] = "0 */1 * * *",         // Every hour
    ["TimeZoneId"] = "UTC"
};

var monthlyTrigger = new Dictionary<string, object>
{
    ["CronExpression"] = "0 0 1 * *",           // First of month at midnight
    ["TimeZoneId"] = "Europe/London"
};
```

## Architecture Notes

### Separation of Concerns
The scheduling system follows clear separation:
- **Scheduling (WHEN)**: Handled by `IFractalScheduler` and trigger types
- **Execution (HOW)**: Handled by `IFractalScheduledExecutionHandler`
- **Service Layer**: `IFractalSchedulingService` provides application-facing API

### Cron Expression Features
Using the Cronos library, the system supports:
- Standard 5-field cron expressions (minute precision)
- Extended 6-field cron expressions (second precision)
- Cron descriptors (`@yearly`, `@monthly`, `@daily`, `@hourly`)
- Timezone-aware calculations with DST handling
- Comprehensive validation and error reporting

### Enhanced Enum Pattern
Trigger types use the enhanced enum pattern for:
- Type-safe trigger type definitions
- Extensible trigger implementations
- Source-generated collections and lookups
- Compile-time validation

## Code Coverage Exclusions

The following should be excluded from coverage testing:

### Helper Methods
- `Schedule.ExtractCronExpression()` - `[ExcludeFromCodeCoverage]` - Helper method tested through factory methods
- `Schedule.ExtractTimeZoneId()` - `[ExcludeFromCodeCoverage]` - Helper method tested through factory methods
- `Schedule.ValidateTriggerConfiguration()` - `[ExcludeFromCodeCoverage]` - Complex trigger validation tested in trigger type tests

### Infrastructure Code
- Source-generated message classes and collections - No business logic to test
- Enhanced enum infrastructure - Framework-level code

## Validation and Error Handling

### Schedule Validation
The `Schedule.Validate()` method performs comprehensive validation:
- Identity validation (IDs and names present)
- Process validation (type and configuration valid)
- Trigger validation (delegates to trigger type)
- Timestamp validation (logical consistency)

### Cron Trigger Validation
The `Cron.ValidateTrigger()` method validates:
- Cron expression format using Cronos library
- Timezone identifier existence
- Configuration completeness
- Future execution possibility (expression not expired)

### Error Result Patterns
All operations use `IFdwResult` for consistent error handling:
- Success/failure indication
- Detailed error messages
- Exception wrapping where appropriate
- Validation error aggregation

## Integration Points

### Execution Handler Integration
Schedulers work with `IFractalScheduledExecutionHandler` to:
- Execute scheduled processes when triggers fire
- Handle execution context and metadata
- Track execution results and history

### Service Factory Integration
Service types support factory patterns for:
- Dynamic service creation
- Configuration-driven service selection
- Dependency injection integration

## Extension Points

### Custom Trigger Types
New trigger types can be created by:
1. Extending `TriggerTypeBase`
2. Implementing `CalculateNextExecution()` and `ValidateTrigger()`
3. Adding `[EnumOption]` attribute
4. Registering with enhanced enum system

### Custom Schedule Properties
Schedules support extensible metadata through:
- `Metadata` dictionary property
- Type-safe configuration objects
- Custom validation logic in derived classes

## Performance Considerations

- Cron calculations use efficient Cronos library
- Timezone conversions handled by .NET TimeZoneInfo
- Enhanced enums provide compile-time optimization
- Source generators eliminate runtime reflection
- Immutable schedule data reduces memory pressure