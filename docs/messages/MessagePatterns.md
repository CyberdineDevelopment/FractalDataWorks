# Message Patterns and Best Practices

This guide covers common patterns and best practices for designing effective message classes.

## Message Types

### Error Messages
Used for error conditions and failures.

```csharp
[Message("UserNotFound")]
public sealed class UserNotFoundMessage : ServiceMessage
{
    public UserNotFoundMessage() 
        : base(1001, "UserNotFound", MessageSeverity.Error, 
               "User not found", "USER_NOT_FOUND") { }

    public UserNotFoundMessage(int userId) 
        : base(1001, "UserNotFound", MessageSeverity.Error, 
               $"User with ID {userId} not found", "USER_NOT_FOUND") { }
}
```

### Warning Messages
Used for non-critical issues that should be noted.

```csharp
[Message("DeprecatedFeature")]
public sealed class DeprecatedFeatureMessage : ServiceMessage
{
    public DeprecatedFeatureMessage() 
        : base(2001, "DeprecatedFeature", MessageSeverity.Warning, 
               "Using deprecated feature", "DEPRECATED_FEATURE") { }

    public DeprecatedFeatureMessage(string featureName) 
        : base(2001, "DeprecatedFeature", MessageSeverity.Warning, 
               $"Feature '{featureName}' is deprecated", "DEPRECATED_FEATURE") { }

    public DeprecatedFeatureMessage(string featureName, string replacement) 
        : base(2001, "DeprecatedFeature", MessageSeverity.Warning, 
               $"Feature '{featureName}' is deprecated. Use '{replacement}' instead", "DEPRECATED_FEATURE") { }
}
```

### Information Messages
Used for informational notifications.

```csharp
[Message("OperationCompleted")]
public sealed class OperationCompletedMessage : ServiceMessage
{
    public OperationCompletedMessage() 
        : base(3001, "OperationCompleted", MessageSeverity.Information, 
               "Operation completed successfully", "OPERATION_COMPLETED") { }

    public OperationCompletedMessage(string operationName) 
        : base(3001, "OperationCompleted", MessageSeverity.Information, 
               $"Operation '{operationName}' completed successfully", "OPERATION_COMPLETED") { }

    public OperationCompletedMessage(string operationName, TimeSpan duration) 
        : base(3001, "OperationCompleted", MessageSeverity.Information, 
               $"Operation '{operationName}' completed in {duration.TotalMilliseconds}ms", "OPERATION_COMPLETED") { }
}
```

## Constructor Patterns

### Progressive Detail Pattern
Start with basic message, add more context with additional constructors.

```csharp
[Message("ValidationFailed")]
public sealed class ValidationFailedMessage : ServiceMessage
{
    // Basic validation failure
    public ValidationFailedMessage() 
        : base(1002, "ValidationFailed", MessageSeverity.Error, 
               "Validation failed", "VALIDATION_FAILED") { }

    // With field name
    public ValidationFailedMessage(string fieldName) 
        : base(1002, "ValidationFailed", MessageSeverity.Error, 
               $"Validation failed for field '{fieldName}'", "VALIDATION_FAILED") { }

    // With field name and specific error
    public ValidationFailedMessage(string fieldName, string error) 
        : base(1002, "ValidationFailed", MessageSeverity.Error, 
               $"Validation failed for field '{fieldName}': {error}", "VALIDATION_FAILED") { }

    // With multiple errors
    public ValidationFailedMessage(string[] errors) 
        : base(1002, "ValidationFailed", MessageSeverity.Error, 
               $"Validation failed: {string.Join("; ", errors)}", "VALIDATION_FAILED") { }
}
```

### Entity Context Pattern
Provide different ways to identify the same entity.

```csharp
[Message("ResourceNotFound")]
public sealed class ResourceNotFoundMessage : ServiceMessage
{
    // Generic resource not found
    public ResourceNotFoundMessage() 
        : base(1003, "ResourceNotFound", MessageSeverity.Error, 
               "Resource not found", "RESOURCE_NOT_FOUND") { }

    // By integer ID
    public ResourceNotFoundMessage(int id) 
        : base(1003, "ResourceNotFound", MessageSeverity.Error, 
               $"Resource with ID {id} not found", "RESOURCE_NOT_FOUND") { }

    // By string identifier
    public ResourceNotFoundMessage(string identifier) 
        : base(1003, "ResourceNotFound", MessageSeverity.Error, 
               $"Resource '{identifier}' not found", "RESOURCE_NOT_FOUND") { }

    // By type and ID
    public ResourceNotFoundMessage(string resourceType, int id) 
        : base(1003, "ResourceNotFound", MessageSeverity.Error, 
               $"{resourceType} with ID {id} not found", "RESOURCE_NOT_FOUND") { }

    // By type and identifier
    public ResourceNotFoundMessage(string resourceType, string identifier) 
        : base(1003, "ResourceNotFound", MessageSeverity.Error, 
               $"{resourceType} '{identifier}' not found", "RESOURCE_NOT_FOUND") { }
}
```

## Message ID Patterns

### Range-Based IDs
Use consistent ID ranges for different message categories:

```csharp
// User-related messages: 1000-1999
public const int USER_NOT_FOUND = 1001;
public const int USER_ALREADY_EXISTS = 1002;
public const int USER_PERMISSION_DENIED = 1003;

// Configuration messages: 2000-2999  
public const int CONFIG_NOT_INITIALIZED = 2001;
public const int CONFIG_INVALID_VALUE = 2002;
public const int CONFIG_MISSING_SECTION = 2003;

// Service messages: 3000-3999
public const int SERVICE_UNAVAILABLE = 3001;
public const int SERVICE_TIMEOUT = 3002;
public const int SERVICE_INITIALIZATION_FAILED = 3003;
```

### Hierarchical IDs
Use hierarchical numbering for related messages:

```csharp
// Database messages: 4000-4999
public const int DATABASE_CONNECTION_FAILED = 4001;
public const int DATABASE_QUERY_FAILED = 4010;
public const int DATABASE_QUERY_TIMEOUT = 4011;
public const int DATABASE_QUERY_SYNTAX_ERROR = 4012;
public const int DATABASE_TRANSACTION_FAILED = 4020;
public const int DATABASE_TRANSACTION_ROLLED_BACK = 4021;
```

## Error Code Patterns

### Consistent Naming
Use consistent, searchable error codes:

```csharp
// Good: Descriptive and consistent
"USER_NOT_FOUND"
"CONFIG_INVALID_VALUE" 
"DATABASE_CONNECTION_FAILED"

// Bad: Inconsistent and unclear
"USR_NF"
"BadConfig"
"db_error"
```

### Hierarchical Codes
Use prefixes to group related errors:

```csharp
// Authentication errors
"AUTH_USER_NOT_FOUND"
"AUTH_INVALID_CREDENTIALS"
"AUTH_TOKEN_EXPIRED"

// Validation errors
"VALIDATION_REQUIRED_FIELD"
"VALIDATION_INVALID_FORMAT"
"VALIDATION_OUT_OF_RANGE"

// External service errors
"EXTERNAL_SERVICE_UNAVAILABLE"
"EXTERNAL_SERVICE_TIMEOUT"
"EXTERNAL_SERVICE_INVALID_RESPONSE"
```

## Collection Organization

### By Domain
Organize collections by business domain:

```csharp
[MessageCollection("UserMessages", ReturnType = typeof(IServiceMessage))]
public abstract class UserMessageCollectionBase : MessageCollectionBase<ServiceMessage> { }

[MessageCollection("OrderMessages", ReturnType = typeof(IServiceMessage))]
public abstract class OrderMessageCollectionBase : MessageCollectionBase<ServiceMessage> { }

[MessageCollection("PaymentMessages", ReturnType = typeof(IServiceMessage))]
public abstract class PaymentMessageCollectionBase : MessageCollectionBase<ServiceMessage> { }
```

### By Severity
Organize collections by message severity:

```csharp
[MessageCollection("ErrorMessages", ReturnType = typeof(IServiceMessage))]
public abstract class ErrorMessageCollectionBase : MessageCollectionBase<ServiceMessage> { }

[MessageCollection("WarningMessages", ReturnType = typeof(IServiceMessage))]
public abstract class WarningMessageCollectionBase : MessageCollectionBase<ServiceMessage> { }
```

### By Layer
Organize collections by architectural layer:

```csharp
[MessageCollection("ServiceMessages", ReturnType = typeof(IServiceMessage))]
public abstract class ServiceMessageCollectionBase : MessageCollectionBase<ServiceMessage> { }

[MessageCollection("RepositoryMessages", ReturnType = typeof(IServiceMessage))]
public abstract class RepositoryMessageCollectionBase : MessageCollectionBase<ServiceMessage> { }

[MessageCollection("ApiMessages", ReturnType = typeof(IServiceMessage))]
public abstract class ApiMessageCollectionBase : MessageCollectionBase<ServiceMessage> { }
```

## Advanced Patterns

### Contextual Messages
Messages that include execution context:

```csharp
[Message("OperationFailed")]
public sealed class OperationFailedMessage : ServiceMessage
{
    public OperationFailedMessage(string operation, Exception exception) 
        : base(4001, "OperationFailed", MessageSeverity.Error, 
               $"Operation '{operation}' failed: {exception.Message}", 
               "OPERATION_FAILED") { }

    public OperationFailedMessage(string operation, string context, Exception exception) 
        : base(4001, "OperationFailed", MessageSeverity.Error, 
               $"Operation '{operation}' failed in context '{context}': {exception.Message}", 
               "OPERATION_FAILED") { }
}
```

### Parameterized Messages
Messages with structured parameters:

```csharp
[Message("RateLimitExceeded")]
public sealed class RateLimitExceededMessage : ServiceMessage
{
    public RateLimitExceededMessage(int currentCount, int maxCount, TimeSpan resetTime) 
        : base(4002, "RateLimitExceeded", MessageSeverity.Warning, 
               $"Rate limit exceeded: {currentCount}/{maxCount}. Resets in {resetTime.TotalMinutes:F0} minutes", 
               "RATE_LIMIT_EXCEEDED") { }
}
```

## Testing Message Classes

```csharp
[Test]
public void ValidationFailedMessage_Should_Generate_Correct_Methods()
{
    // Test parameterless constructor
    var basicMessage = AppMessages.ValidationFailed();
    basicMessage.Message.Should().Be("Validation failed");
    basicMessage.Code.Should().Be("VALIDATION_FAILED");

    // Test single parameter constructor
    var fieldMessage = AppMessages.ValidationFailed("Email");
    fieldMessage.Message.Should().Be("Validation failed for field 'Email'");

    // Test two parameter constructor
    var detailMessage = AppMessages.ValidationFailed("Email", "Invalid format");
    detailMessage.Message.Should().Be("Validation failed for field 'Email': Invalid format");
}
```

This pattern-based approach ensures consistent, maintainable, and discoverable message implementations across your application.