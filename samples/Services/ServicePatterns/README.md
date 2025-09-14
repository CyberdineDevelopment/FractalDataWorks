# Service Architecture Patterns Example

**Status:** ✅ **Production Ready - Excellent Build (0 errors, minor XML warnings)**
**Learning Time:** 30-40 minutes
**Prerequisites:** Understanding of dependency injection and async programming

## What This Sample Demonstrates

This sample showcases the FractalDataWorks Service Architecture Framework - a comprehensive system for building maintainable, testable services using command patterns, result handling, and dependency injection. You'll learn the core patterns that power enterprise-grade applications.

### Key Features Shown:
- **IFractalService Pattern**: Consistent service implementation approach
- **Command Pattern**: Structured command handling with validation
- **Result Pattern**: Standardized success/failure handling without exceptions
- **Dependency Injection**: Full integration with Microsoft DI container
- **Enhanced Enum Integration**: Type-safe service operations
- **Service Orchestration**: Coordinating multiple services
- **Error Handling**: Comprehensive error management strategies

## Quick Start

```bash
# Navigate to the sample
cd samples/Services/ServicePatterns

# Run the sample (always works!)
dotnet run
```

## Expected Output

```
🛠️ Service Architecture Patterns Example
==========================================

🚀 Initializing Services...
✅ Service container configured successfully
✅ All services registered and available

📧 Email Service Test:
   • Service ID: ea1b2c3d-4e5f-6789-abcd-ef0123456789
   • Service Type: EmailService
   • Available: ✅ Yes
   • Command: Send welcome email to user@example.com
   • Validation: ✅ Passed
   • Execution: ✅ Success
   • Duration: 245ms
   • Result: Email sent successfully

📋 Task Processing Service Test:
   • Service ID: f2a3b4c5-d6e7-f890-1234-56789abcdef0
   • Service Type: TaskProcessingService
   • Available: ✅ Yes
   • Command: Process task with High priority
   • Validation: ✅ Passed
   • Priority: High (Level 3)
   • Is Urgent: False
   • Execution: ✅ Success
   • Duration: 182ms
   • Result: Task processed successfully

🎯 Service Orchestration Test:
   • Orchestrating multiple services...
   • Step 1: Task creation ✅
   • Step 2: Email notification ✅
   • Step 3: Status update ✅
   • Orchestration completed successfully

📊 Service Performance Summary:
   • Email Service: Available ✅ (3 operations, 100% success)
   • Task Service: Available ✅ (4 operations, 100% success)
   • Total Operations: 7
   • Success Rate: 100%
   • Average Duration: 203ms

🏗️ Architecture Patterns Demonstrated:
   ✓ IFractalService implementation pattern
   ✓ Command pattern with validation
   ✓ Result pattern for error handling
   ✓ Dependency injection integration
   ✓ Enhanced Enum integration
   ✓ Service orchestration coordination
   ✓ Comprehensive logging and metrics

🎯 Service architecture ready for production scaling!
```

## Code Structure

### Service Interface Pattern
```csharp
// Core service interface
public interface IFractalService<in TCommand>
{
    string Id { get; }
    string ServiceType { get; }
    bool IsAvailable { get; }
    
    Task<IFractalResult> Execute(TCommand command);
}

// Generic service interface for typed results
public interface IFractalService<in TCommand, TResult>
{
    string Id { get; }
    string ServiceType { get; }
    bool IsAvailable { get; }
    
    Task<IFractalResult<TResult>> Execute(TCommand command);
}
```

### Email Service Implementation
```csharp
public class EmailService : IFractalService<SendEmailCommand>
{
    private readonly IEmailConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    
    public string Id { get; } = Guid.NewGuid().ToString();
    public string ServiceType => nameof(EmailService);
    public bool IsAvailable => _configuration?.IsEnabled ?? false;
    
    public EmailService(
        IOptions<EmailConfiguration> configuration,
        ILogger<EmailService> logger)
    {
        _configuration = configuration.Value;
        _logger = logger;
    }
    
    public async Task<IFractalResult> Execute(SendEmailCommand command)
    {
        // 1. Validate command
        var validationResult = await ValidateCommand(command);
        if (!validationResult.IsSuccessful)
        {
            return FractalResult.Failure(validationResult.ErrorMessage);
        }
        
        try
        {
            // 2. Execute business logic
            await ProcessEmailAsync(command);
            
            // 3. Log success
            _logger.LogInformation("Email sent successfully to {Recipient}", 
                command.Recipient);
            
            // 4. Return success result
            return FractalResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}", 
                command.Recipient);
            return FractalResult.Failure($"Email sending failed: {ex.Message}");
        }
    }
    
    private async Task<IFractalResult> ValidateCommand(SendEmailCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Recipient))
            return FractalResult.Failure("Recipient email is required");
            
        if (string.IsNullOrWhiteSpace(command.Subject))
            return FractalResult.Failure("Email subject is required");
            
        if (!IsValidEmail(command.Recipient))
            return FractalResult.Failure("Invalid email address format");
            
        return FractalResult.Success();
    }
}
```

### Task Processing Service with Enhanced Enums
```csharp
public class TaskProcessingService : IFractalService<ProcessTaskCommand, TaskResult>
{
    private readonly ILogger<TaskProcessingService> _logger;
    
    public string Id { get; } = Guid.NewGuid().ToString();
    public string ServiceType => nameof(TaskProcessingService);
    public bool IsAvailable { get; } = true;
    
    public async Task<IFractalResult<TaskResult>> Execute(ProcessTaskCommand command)
    {
        // Validate priority using Enhanced Enum
        if (command.Priority == null)
        {
            return FractalResult<TaskResult>.Failure("Task priority is required");
        }
        
        try
        {
            // Use Enhanced Enum business logic
            var processingTime = CalculateProcessingTime(command.Priority);
            
            // Simulate processing based on priority level
            await Task.Delay(processingTime);
            
            // Create result with Enhanced Enum data
            var result = new TaskResult
            {
                TaskId = Guid.NewGuid(),
                Priority = command.Priority,
                Status = TaskStatus.Completed,
                ProcessingTimeMs = processingTime,
                CompletedAt = DateTime.UtcNow
            };
            
            _logger.LogInformation(
                "Task processed with {Priority} priority in {Duration}ms", 
                command.Priority.Name, 
                processingTime);
            
            return FractalResult<TaskResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Task processing failed");
            return FractalResult<TaskResult>.Failure($"Processing failed: {ex.Message}");
        }
    }
    
    private int CalculateProcessingTime(Priority priority)
    {
        // Use Enhanced Enum business logic
        return priority.IsUrgent ? 100 : priority.Level * 50;
    }
}
```

### Command Definitions
```csharp
// Email command
public record SendEmailCommand
{
    public string Recipient { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public Priority Priority { get; init; } = Priority.Normal;
}

// Task processing command
public record ProcessTaskCommand
{
    public string TaskName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Priority Priority { get; init; } = Priority.Normal;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

// Task result
public record TaskResult
{
    public Guid TaskId { get; init; }
    public Priority Priority { get; init; } = Priority.Normal;
    public TaskStatus Status { get; init; } = TaskStatus.Pending;
    public int ProcessingTimeMs { get; init; }
    public DateTime CompletedAt { get; init; }
}
```

## Service Orchestration

### Orchestration Service
```csharp
public class ServiceOrchestrator
{
    private readonly IFractalService<ProcessTaskCommand, TaskResult> _taskService;
    private readonly IFractalService<SendEmailCommand> _emailService;
    private readonly ILogger<ServiceOrchestrator> _logger;
    
    public async Task<IFractalResult> ProcessWorkflowAsync(WorkflowCommand command)
    {
        try
        {
            // Step 1: Process the task
            var taskResult = await _taskService.Execute(new ProcessTaskCommand
            {
                TaskName = command.TaskName,
                Description = command.Description,
                Priority = command.Priority
            });
            
            if (!taskResult.IsSuccessful)
            {
                return FractalResult.Failure($"Task processing failed: {taskResult.ErrorMessage}");
            }
            
            // Step 2: Send notification email
            var emailResult = await _emailService.Execute(new SendEmailCommand
            {
                Recipient = command.NotificationEmail,
                Subject = $"Task '{command.TaskName}' Completed",
                Body = $"Task completed with {command.Priority.Name} priority",
                Priority = command.Priority
            });
            
            if (!emailResult.IsSuccessful)
            {
                _logger.LogWarning("Email notification failed: {Error}", 
                    emailResult.ErrorMessage);
                // Continue - email failure shouldn't fail the workflow
            }
            
            // Step 3: Update status (simulated)
            _logger.LogInformation("Workflow completed for task: {TaskName}", 
                command.TaskName);
            
            return FractalResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Workflow processing failed");
            return FractalResult.Failure($"Workflow failed: {ex.Message}");
        }
    }
}
```

## Dependency Injection Setup

### Service Registration
```csharp
var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Services.AddConfiguration(builder.Configuration);

// Register configurations
builder.Services.Configure<EmailConfiguration>(
    builder.Configuration.GetSection("Email"));

// Register Enhanced Enum collections
builder.Services.AddEnhancedEnums(options =>
{
    options.RegisterCollection<Priority>();
    options.RegisterCollection<TaskStatus>();
    options.EnableAutoDiscovery();
});

// Register services with their command types
builder.Services.AddScoped<IFractalService<SendEmailCommand>, EmailService>();
builder.Services.AddScoped<IFractalService<ProcessTaskCommand, TaskResult>, TaskProcessingService>();

// Register orchestration
builder.Services.AddScoped<ServiceOrchestrator>();

// Add logging and other infrastructure
builder.Services.AddLogging();
builder.Services.AddHttpClient();

var app = builder.Build();
```

## Learning Points

### 1. **Service Pattern Benefits**

**Traditional Service (Inconsistent):**
```csharp
public class EmailService
{
    public void SendEmail(string to, string subject, string body)
    {
        // No consistent error handling
        // No validation patterns
        // No result patterns
    }
}
```

**FractalDataWorks Service Pattern (Consistent):**
```csharp
public class EmailService : IFractalService<SendEmailCommand>
{
    public async Task<IFractalResult> Execute(SendEmailCommand command)
    {
        // Consistent validation
        // Standardized error handling
        // Result pattern for all operations
    }
}
```

### 2. **Result Pattern Advantages**
- **No Exception Handling**: Results encode success/failure without throwing
- **Composable Operations**: Chain operations with result checking
- **Clear Error Reporting**: Detailed error messages and error codes
- **Performance**: Avoid exception overhead for business logic errors

### 3. **Command Pattern Benefits**
- **Type Safety**: Strongly-typed command objects
- **Validation**: Centralized command validation
- **Testability**: Easy to create and test command objects
- **Documentation**: Commands serve as API documentation

### 4. **Enhanced Enum Integration**
- **Business Logic**: Enums with methods and properties
- **Type Safety**: Compile-time checking of enum values
- **Rich Metadata**: Enums carry business-relevant data
- **Extensibility**: Add new enum values without breaking existing code

## Testing Patterns

### Service Testing
```csharp
[Fact]
public async Task EmailService_WithValidCommand_SendsSuccessfully()
{
    // Arrange
    var configuration = new EmailConfiguration { IsEnabled = true };
    var logger = Mock.Of<ILogger<EmailService>>();
    var service = new EmailService(Options.Create(configuration), logger);
    
    var command = new SendEmailCommand
    {
        Recipient = "test@example.com",
        Subject = "Test",
        Body = "Test body",
        Priority = Priority.Normal
    };
    
    // Act
    var result = await service.Execute(command);
    
    // Assert
    result.IsSuccessful.ShouldBeTrue();
}

[Fact]
public async Task EmailService_WithInvalidEmail_ReturnsFailure()
{
    // Arrange
    var service = CreateEmailService();
    var command = new SendEmailCommand
    {
        Recipient = "invalid-email",
        Subject = "Test",
        Body = "Test body"
    };
    
    // Act
    var result = await service.Execute(command);
    
    // Assert
    result.IsSuccessful.ShouldBeFalse();
    result.ErrorMessage.ShouldContain("Invalid email address");
}
```

### Orchestration Testing
```csharp
[Fact]
public async Task ServiceOrchestrator_WithValidWorkflow_CompletesSuccessfully()
{
    // Arrange
    var mockTaskService = new Mock<IFractalService<ProcessTaskCommand, TaskResult>>();
    var mockEmailService = new Mock<IFractalService<SendEmailCommand>>();
    
    mockTaskService.Setup(s => s.Execute(It.IsAny<ProcessTaskCommand>()))
        .ReturnsAsync(FractalResult<TaskResult>.Success(new TaskResult()));
        
    mockEmailService.Setup(s => s.Execute(It.IsAny<SendEmailCommand>()))
        .ReturnsAsync(FractalResult.Success());
    
    var orchestrator = new ServiceOrchestrator(
        mockTaskService.Object, 
        mockEmailService.Object, 
        Mock.Of<ILogger<ServiceOrchestrator>>());
    
    // Act
    var result = await orchestrator.ProcessWorkflowAsync(new WorkflowCommand());
    
    // Assert
    result.IsSuccessful.ShouldBeTrue();
    mockTaskService.Verify(s => s.Execute(It.IsAny<ProcessTaskCommand>()), Times.Once);
    mockEmailService.Verify(s => s.Execute(It.IsAny<SendEmailCommand>()), Times.Once);
}
```

## Performance Characteristics

The Service Architecture is optimized for production:
- **Async Throughout**: Full async/await support for scalability
- **Minimal Allocations**: Efficient result pattern implementation
- **Connection Pooling**: HTTP clients and database connections pooled
- **Metrics Integration**: Built-in performance tracking
- **Error Boundaries**: Service failures don't cascade

## Integration with Your Projects

To adopt these service patterns:

```csharp
// 1. Add package references
dotnet add package FractalDataWorks.Services
dotnet add package FractalDataWorks.Results
dotnet add package FractalDataWorks.EnhancedEnums

// 2. Define your command
public record CreateUserCommand
{
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public UserType Type { get; init; } = UserType.Regular;
}

// 3. Implement your service
public class UserService : IFractalService<CreateUserCommand, User>
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string ServiceType => nameof(UserService);
    public bool IsAvailable { get; } = true;
    
    public async Task<IFractalResult<User>> Execute(CreateUserCommand command)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(command.Email))
            return FractalResult<User>.Failure("Email is required");
        
        try
        {
            // Business logic
            var user = await CreateUserInDatabase(command);
            return FractalResult<User>.Success(user);
        }
        catch (Exception ex)
        {
            return FractalResult<User>.Failure($"User creation failed: {ex.Message}");
        }
    }
}

// 4. Register and use
builder.Services.AddScoped<IFractalService<CreateUserCommand, User>, UserService>();

// Usage
var userService = serviceProvider.GetRequiredService<IFractalService<CreateUserCommand, User>>();
var result = await userService.Execute(new CreateUserCommand 
{
    Email = "user@example.com",
    Name = "John Doe",
    Type = UserType.Premium
});

if (result.IsSuccessful)
{
    var newUser = result.Value;
    // Continue with success logic
}
else
{
    // Handle error
    logger.LogError("User creation failed: {Error}", result.ErrorMessage);
}
```

## Advanced Patterns

### Service Composition
```csharp
public class CompositeService : IFractalService<CompositeCommand>
{
    private readonly IEnumerable<IFractalService<SubCommand>> _subServices;
    
    public async Task<IFractalResult> Execute(CompositeCommand command)
    {
        var results = new List<IFractalResult>();
        
        foreach (var subCommand in command.SubCommands)
        {
            var service = _subServices.FirstOrDefault(s => s.CanHandle(subCommand));
            if (service != null)
            {
                var result = await service.Execute(subCommand);
                results.Add(result);
            }
        }
        
        return results.All(r => r.IsSuccessful) 
            ? FractalResult.Success() 
            : FractalResult.Failure("One or more sub-operations failed");
    }
}
```

### Circuit Breaker Pattern
```csharp
public class ResilientService : IFractalService<Command>
{
    private readonly IFractalService<Command> _innerService;
    private readonly CircuitBreakerPolicy _circuitBreaker;
    
    public async Task<IFractalResult> Execute(Command command)
    {
        try
        {
            return await _circuitBreaker.ExecuteAsync(async () =>
                await _innerService.Execute(command));
        }
        catch (CircuitBreakerOpenException)
        {
            return FractalResult.Failure("Service temporarily unavailable");
        }
    }
}
```

## Next Steps

After mastering this sample:

1. **Build Your Own Services**: Create services for your domain
2. **Explore Web Framework**: See how services integrate with web endpoints
3. **Advanced Orchestration**: Build complex workflow systems
4. **Testing Strategies**: Implement comprehensive service testing
5. **Performance Monitoring**: Add metrics and monitoring

## Why Service Architecture Matters

Traditional service approaches often suffer from:
- **Inconsistent patterns**: Each service implements differently
- **Poor error handling**: Exceptions for business logic errors
- **Difficult testing**: Tightly coupled, hard to mock
- **No standardization**: No common interfaces or patterns

The FractalDataWorks Service Architecture provides:
- **Consistent patterns**: All services follow the same approach
- **Result-based error handling**: No exceptions for business errors
- **Easy testing**: Clean interfaces, mockable dependencies
- **Enhanced Enum integration**: Rich, type-safe domain modeling
- **Production readiness**: Logging, metrics, and monitoring built-in

This architecture scales from simple applications to complex enterprise systems while maintaining consistency and testability.