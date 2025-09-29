# FractalDataWorks.Services.Execution.Abstractions

## Overview

The **FractalDataWorks.Services.Execution.Abstractions** project provides the foundational contracts and base types for process execution within the FractalDataWorks framework. This project defines the abstractions that enable different types of processes (ETL, data migration, batch processing, etc.) to be executed, monitored, and managed through a unified process execution system.

## Purpose

This abstractions library provides:

- **Process Contracts** - Core interfaces for process definition and execution
- **State Management** - Process state tracking and transition definitions
- **Result Models** - Standardized result and metrics reporting
- **Message System** - Process execution messages and notifications
- **Base Types** - Enhanced enum base classes for process types and states
- **Configuration Abstractions** - Process configuration and parameter management

## Key Components

### Core Interfaces

#### `IProcess`
**File**: `Interfaces/IProcess.cs`

Core interface for all executable processes within the FractalDataWorks framework.

**Key Features**:
- Process identification and metadata
- State management and transition events
- Operation execution with cancellation support
- Configuration and parameter management
- Error handling and reporting
- Process lifecycle management

**Methods**:
- `ExecuteAsync(operationName, cancellationToken)` - Execute named operations
- `GetState()` - Retrieve current process state
- `GetConfiguration<T>()` - Access typed configuration
- `Cancel()` - Request process cancellation
- `Dispose()` - Clean up process resources

#### `IProcessResult`
**File**: `Interfaces/IProcessResult.cs`

Interface for process execution results with comprehensive outcome information.

**Properties**:
- `IsSuccess` - Indicates successful execution
- `ProcessId` - Associated process identifier
- `OperationName` - Executed operation name
- `StartTime` / `EndTime` - Execution timing
- `Message` - Result message or error description
- `Metrics` - Detailed execution metrics
- `Data` - Optional result data

#### `IProcessMetrics`
**File**: `Interfaces/IProcessMetrics.cs`

Interface for process execution metrics and performance data.

**Metrics Include**:
- Execution duration and timing breakdowns
- Resource utilization (memory, CPU)
- Throughput and processing rates
- Error counts and retry statistics
- Custom process-specific metrics

### Model Classes

#### `ProcessResult`
**File**: `Models/ProcessResult.cs`

Concrete implementation of `IProcessResult` with comprehensive result information.

**Features**:
- Success/failure state tracking
- Detailed error information
- Execution timing and performance data
- Structured result data with type safety
- Serialization support for result persistence

#### `ProcessMetrics`
**File**: `Models/ProcessMetrics.cs`

Concrete implementation of `IProcessMetrics` with detailed performance tracking.

**Capabilities**:
- Real-time metric collection
- Historical metric storage
- Aggregated metric calculations
- Performance trend analysis
- Custom metric registration

### Enhanced Enums Base Classes

#### `ProcessTypeBase`
**File**: `EnhancedEnums/ProcessTypeBase.cs`

Abstract base class for defining process types using the enhanced enum pattern.

**Features**:
- Type-safe process type definitions
- Process capability descriptions
- Supported operation definitions
- Configuration schema definitions
- Process factory method abstractions

#### `ProcessTypeCollectionBase`
**File**: `EnhancedEnums/ProcessTypeCollectionBase.cs`

Base class for source generator collections of process types.

#### `ProcessStateBase`
**File**: `EnhancedEnums/ProcessStateBase.cs`

Abstract base class for defining process states using the enhanced enum pattern.

**Features**:
- State identification and descriptions
- Valid state transitions
- State-specific behaviors
- Timeout and lifecycle management

#### `ProcessStateCollectionBase`
**File**: `EnhancedEnums/ProcessStateCollectionBase.cs`

Base class for source generator collections of process states.

#### `IProcessType` and `IProcessState`
**Files**: `EnhancedEnums/IProcessType.cs`, `EnhancedEnums/IProcessState.cs`

Interfaces for process types and states with enhanced enum support.

### Standard Process States

#### `Created`
**File**: `EnhancedEnums/States/Created.cs`

Initial state for newly created process instances.

#### `Running`
**File**: `EnhancedEnums/States/Running.cs`

State for processes currently executing operations.

#### `Completed`
**File**: `EnhancedEnums/States/Completed.cs`

Final state for successfully completed processes.

#### `Failed`
**File**: `EnhancedEnums/States/Failed.cs`

Final state for processes that failed during execution.

#### `Cancelled`
**File**: `EnhancedEnums/States/Cancelled.cs`

Final state for processes that were cancelled before completion.

### Message System

#### `ExecutionMessage`
**File**: `Messages/ExecutionMessage.cs`

Base class for all process execution messages.

#### `ExecutionMessageCollectionBase`
**File**: `Messages/ExecutionMessageCollectionBase.cs`

Base class for execution message collections using source generators.

#### Configuration Messages

- `ProcessConfigurationInvalidMessage` - Configuration validation failures
- `ProcessConfigurationMissingMessage` - Missing required configuration

#### Process Messages

- `ProcessCancellationRequestedMessage` - Process cancellation notifications
- `ProcessTimeoutMessage` - Process timeout notifications
- `ProcessStateTransitionFailedMessage` - Invalid state transitions
- `ProcessExecutionFailedMessage` - General process execution failures

#### Execution Messages

- `OperationExecutionStartedMessage` - Operation start notifications
- `OperationExecutionCompletedMessage` - Operation completion notifications
- `OperationExecutionFailedMessage` - Operation failure notifications
- `OperationNotSupportedMessage` - Unsupported operation requests

## Dependencies

### Project References
- `FractalDataWorks.EnhancedEnums` - Enhanced enumeration system
- `FractalDataWorks.EnhancedEnums.SourceGenerators` - Code generation for enums
- `FractalDataWorks.Results` - Result pattern implementation
- `FractalDataWorks.Services.Abstractions` - Base service abstractions
- `FractalDataWorks.Messages` - Message system
- `FractalDataWorks.Messages.SourceGenerators` - Message code generation

### Package References
- `Microsoft.Extensions.DependencyInjection.Abstractions` - DI framework
- `Microsoft.Extensions.Logging.Abstractions` - Logging framework

## Usage Patterns

### Implementing Custom Process Types

```csharp
public sealed class MyCustomProcessType : ProcessTypeBase<MyCustomProcessType>, 
    IEnumOption<MyCustomProcessType>
{
    public MyCustomProcessType() : base(
        id: 100,
        name: "MyCustomProcess",
        description: "Custom business process implementation",
        supportedOperations: ["Initialize", "Execute", "Finalize"],
        defaultTimeout: TimeSpan.FromMinutes(30))
    {
    }
    
    public override IProcess CreateProcess(
        string processId, 
        object configuration, 
        IServiceProvider serviceProvider)
    {
        return new MyCustomProcess(processId, configuration, serviceProvider);
    }
}
```

### Implementing Custom Process States

```csharp
public sealed class ProcessingState : ProcessStateBase<ProcessingState>, 
    IEnumOption<ProcessingState>
{
    public ProcessingState() : base(
        id: 10,
        name: "Processing",
        description: "Process is actively processing data",
        isTerminal: false,
        allowedTransitions: ["Completed", "Failed", "Cancelled"])
    {
    }
    
    public override bool CanTransitionTo(string targetState)
    {
        return AllowedTransitions.Contains(targetState);
    }
}
```

### Creating Custom Processes

```csharp
public class MyCustomProcess : IProcess
{
    public string ProcessId { get; }
    public string ProcessType => "MyCustomProcess";
    public string CurrentState { get; private set; }
    public object Configuration { get; }
    
    public MyCustomProcess(string processId, object configuration, IServiceProvider serviceProvider)
    {
        ProcessId = processId;
        Configuration = configuration;
        CurrentState = "Created";
        _serviceProvider = serviceProvider;
    }
    
    public async Task<IGenericResult<IProcessResult>> ExecuteAsync(
        string operationName, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var startTime = DateTimeOffset.UtcNow;
            TransitionState("Running");
            
            // Execute operation-specific logic
            var result = await ExecuteOperation(operationName, cancellationToken);
            
            var endTime = DateTimeOffset.UtcNow;
            TransitionState("Completed");
            
            return GenericResult<IProcessResult>.Success(new ProcessResult
            {
                IsSuccess = true,
                ProcessId = ProcessId,
                OperationName = operationName,
                StartTime = startTime,
                EndTime = endTime,
                Metrics = CollectMetrics()
            });
        }
        catch (Exception ex)
        {
            TransitionState("Failed");
            return GenericResult<IProcessResult>.Failure(ex.Message);
        }
    }
    
    private void TransitionState(string newState)
    {
        var previousState = CurrentState;
        CurrentState = newState;
        StateChanged?.Invoke(this, previousState, newState);
    }
    
    public event Action<IProcess, string, string>? StateChanged;
}
```

### Using Process Results and Metrics

```csharp
public class ProcessMonitor
{
    public async Task MonitorProcess(IProcess process)
    {
        // Subscribe to state changes
        process.StateChanged += OnStateChanged;
        
        // Execute process
        var result = await process.ExecuteAsync("MainOperation");
        
        if (result.IsSuccess)
        {
            var metrics = result.Value.Metrics;
            Console.WriteLine($"Process completed in {metrics.ExecutionDuration}");
            Console.WriteLine($"Memory used: {metrics.PeakMemoryUsage}MB");
            Console.WriteLine($"Records processed: {metrics.ItemsProcessed}");
        }
        else
        {
            Console.WriteLine($"Process failed: {result.Message}");
        }
    }
    
    private void OnStateChanged(IProcess process, string previousState, string newState)
    {
        Console.WriteLine($"Process {process.ProcessId} transitioned: {previousState} → {newState}");
        
        if (newState == "Failed")
        {
            // Handle failure scenarios
            NotifyAdministrators(process);
        }
    }
}
```

### Working with Process Messages

```csharp
public class ProcessExecutionHandler
{
    public void HandleExecutionMessages(IEnumerable<ExecutionMessage> messages)
    {
        foreach (var message in messages)
        {
            switch (message)
            {
                case OperationExecutionStartedMessage started:
                    LogOperationStart(started.ProcessId, started.OperationName);
                    break;
                    
                case OperationExecutionCompletedMessage completed:
                    LogOperationCompletion(completed.ProcessId, completed.OperationName, completed.Duration);
                    break;
                    
                case OperationExecutionFailedMessage failed:
                    LogOperationFailure(failed.ProcessId, failed.OperationName, failed.Error);
                    break;
                    
                case ProcessConfigurationInvalidMessage configError:
                    LogConfigurationError(configError.ProcessId, configError.ValidationErrors);
                    break;
            }
        }
    }
}
```

## Code Coverage Exclusions

The following code should be excluded from coverage testing:

### Infrastructure Code
- All message classes in `Messages/` directory
- All classes ending with `CollectionBase.cs`
- Enhanced enum state classes in `EnhancedEnums/States/`

### Abstract Base Classes
- `ProcessTypeBase.cs` - Abstract base with no implementation
- `ProcessStateBase.cs` - Abstract base with no implementation
- `ProcessTypeCollectionBase.cs` - Source generator target
- `ProcessStateCollectionBase.cs` - Source generator target

### Interface Definitions
- All `I*.cs` interface files (no implementation to test)
- Simple property-only model classes

### Model Classes
- Property getter/setter implementations
- `ToString()` method implementations
- Simple constructor parameter assignments

### Message System
- Message base classes and collections
- Simple message property classes

## Implementation Notes

### Process Lifecycle Management
Process execution follows a well-defined lifecycle:
1. **Created** - Process instance created with configuration
2. **Running** - Process actively executing operations
3. **Completed/Failed/Cancelled** - Final states based on execution outcome

### State Transition Rules
- State transitions are enforced through the enhanced enum system
- Each state defines its valid transition targets
- Invalid transitions are prevented and logged as errors
- State changes trigger events for monitoring and logging

### Enhanced Enum Pattern
Process types and states use the enhanced enum pattern:
- Compile-time validation of completeness
- Type-safe access to instances
- Automatic collection generation
- Support for custom behaviors per type/state

### Message-Driven Architecture
Process execution uses structured messaging:
- All significant events generate messages
- Messages are strongly typed with specific data
- Message collections support source generation
- Integration with logging and monitoring systems

### Error Handling Strategy
- Comprehensive error capture at all execution levels
- Structured error information in results
- State transitions on failures
- Support for error recovery and retry scenarios

### Performance Monitoring
- Built-in metrics collection for all processes
- Configurable metric collection granularity
- Performance trend analysis support
- Integration with external monitoring systems

### Thread Safety
All abstractions are designed for concurrent usage:
- Process state management is thread-safe
- Result objects are immutable after creation
- Metrics collection supports concurrent updates
- Event handling is thread-safe

## Target Framework
- **NET 10.0**
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Enabled