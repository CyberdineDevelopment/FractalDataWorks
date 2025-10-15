# Message Pattern - FractalDataWorks Developer Kit

**Created**: 2025-10-14
**Purpose**: Document the CORRECT message pattern for source-generated message collections

---

## ⚠️ IMPORTANT: Two Patterns Exist

There are **TWO** message patterns in the codebase:

1. **OLD PATTERN** (❌ Don't use) - Private nested classes
2. **NEW PATTERN** (✅ Use this) - Public sealed classes with `[Message]` attribute

**This document describes the NEW, CORRECT pattern.**

---

## The Correct Pattern

### Pattern Overview

```
1. Abstract base class extending MessageTemplate<TSeverity>
2. Abstract collection base extending MessageCollectionBase<TMessage> with [MessageCollection]
3. Public sealed message classes with [Message] attribute
4. Source generator creates static properties in generated partial class
```

### File Structure

```
Messages/
├── DataCommandMessage.cs                    ← Base class (abstract)
├── DataCommandMessageCollectionBase.cs      ← Collection base with [MessageCollection]
├── CommandRequiredMessage.cs                ← Concrete message [Message]
├── ContainerNameRequiredMessage.cs          ← Concrete message [Message]
├── TranslationFailedMessage.cs              ← Concrete message [Message]
└── TranslatorNotFoundMessage.cs             ← Concrete message [Message]
```

---

## Step 1: Create Message Base Class

**File**: `Messages/DataCommandMessage.cs`

```csharp
using FractalDataWorks.Messages;

namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Base class for data command messages.
/// </summary>
public abstract class DataCommandMessage : MessageTemplate<MessageSeverity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandMessage"/> class.
    /// </summary>
    protected DataCommandMessage(
        int id,
        string name,
        MessageSeverity severity,
        string message,
        string? code = null)
        : base(id, name, severity, message, code, "DataCommands", null, null)
    {
    }
}
```

**Key Points**:
- Extends `MessageTemplate<MessageSeverity>`
- Protected constructor for concrete classes to call
- Category parameter ("DataCommands") identifies message domain

---

## Step 2: Create Collection Base Class

**File**: `Messages/DataCommandMessageCollectionBase.cs`

```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Collection base for data command messages.
/// Generates static factory methods in DataCommandMessages class.
/// </summary>
[MessageCollection("DataCommandMessages")]
public abstract class DataCommandMessageCollectionBase : MessageCollectionBase<DataCommandMessage>
{
}
```

**Key Points**:
- Has `[MessageCollection("DataCommandMessages")]` attribute
- Extends `MessageCollectionBase<TMessage>`
- **Name matters**: "DataCommandMessages" becomes the generated class name
- Source generator creates a partial class with static properties

---

## Step 3: Create Concrete Message Classes

**File**: `Messages/CommandRequiredMessage.cs`

```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Message indicating that a command is required.
/// </summary>
[Message("CommandRequired")]
public sealed class CommandRequiredMessage : DataCommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandRequiredMessage"/> class.
    /// </summary>
    public CommandRequiredMessage()
        : base(
            id: 1,
            name: "CommandRequired",
            severity: MessageSeverity.Error,
            message: "Command is required",
            code: "DATACMD_001")
    {
    }
}
```

**File**: `Messages/TranslationFailedMessage.cs`

```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Message indicating that command translation failed.
/// </summary>
[Message("TranslationFailed")]
public sealed class TranslationFailedMessage : DataCommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TranslationFailedMessage"/> class.
    /// </summary>
    public TranslationFailedMessage()
        : base(
            id: 100,
            name: "TranslationFailed",
            severity: MessageSeverity.Error,
            message: "Failed to translate {0} command: {1}",
            code: "DATACMD_100")
    {
    }
}
```

**Key Points**:
- `sealed class` - cannot be inherited
- **PUBLIC** constructor (not private!)
- `[Message("MessageName")]` attribute
- Message text can have placeholders: `{0}`, `{1}`, etc.
- Code follows pattern: `PREFIX_###` (e.g., `DATACMD_001`)

---

## Step 4: Source Generator Creates Static Access

The source generator creates:

```csharp
// Auto-generated partial class
public static partial class DataCommandMessages
{
    public static CommandRequiredMessage CommandRequired => new();
    public static ContainerNameRequiredMessage ContainerNameRequired => new();
    public static TranslationFailedMessage TranslationFailed => new();
    public static TranslatorNotFoundMessage TranslatorNotFound => new();
}
```

---

## Usage

### Basic Usage

```csharp
// Return error result with message
return GenericResult.Failure(DataCommandMessages.CommandRequired);

// With parameters
return GenericResult.Failure(
    DataCommandMessages.TranslationFailed.WithParameters(commandType, errorReason));
```

### In Validation

```csharp
public IGenericResult Validate(IDataCommand command)
{
    if (command == null)
        return GenericResult.Failure(DataCommandMessages.CommandRequired);

    if (string.IsNullOrWhiteSpace(command.ContainerName))
        return GenericResult.Failure(DataCommandMessages.ContainerNameRequired);

    return GenericResult.Success();
}
```

### In Translators

```csharp
public async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
    IDataCommand command,
    CancellationToken ct)
{
    try
    {
        // Translation logic...
    }
    catch (Exception ex)
    {
        return GenericResult<IConnectionCommand>.Failure(
            DataCommandMessages.TranslationFailed.WithParameters(command.Name, ex.Message));
    }
}
```

---

## Message Consistency Requirements

### ID Ranges

Organize message IDs by domain:

| Range | Purpose |
|-------|---------|
| 1-99 | General validation errors |
| 100-199 | Translation errors |
| 200-299 | Execution errors |
| 300-399 | Configuration errors |
| 400-499 | Data errors |
| 500-599 | Connection errors |

### Code Prefixes

Use consistent prefixes:

| Prefix | Domain |
|--------|--------|
| `DATACMD_` | Data command messages |
| `SQLCONN_` | SQL connection messages |
| `RESTCONN_` | REST connection messages |
| `FILESVC_` | File service messages |
| `AUTH_` | Authentication messages |

### Message Format

**DO**:
```csharp
message: "Failed to translate {0} command: {1}"  // Parameterized
message: "Container name is required"            // Clear, concise
message: "Translator '{0}' not found"            // Helpful
```

**DON'T**:
```csharp
message: "Error occurred"                        // Too vague
message: "Failed to do the thing"                // Unclear
message: "An error has occurred in the system"   // Verbose, unhelpful
```

### Severity Guidelines

| Severity | Use When |
|----------|----------|
| `Error` | Operation cannot proceed |
| `Warning` | Operation completed but with issues |
| `Information` | Status update |
| `Debug` | Diagnostic information |

---

## ❌ OLD PATTERN (Don't Use)

The old pattern used private nested classes:

```csharp
// ❌ OLD - Don't use this pattern!
[MessageCollection("MyMessages")]
public abstract class MyMessage : MessageTemplate<MessageSeverity>
{
    protected MyMessage(int id, string name, MessageSeverity severity, string message)
        : base(id, name, severity, message, null, "Domain", null, null)
    {
    }
}

public static class MyMessages  // ❌ Static class with nested privates
{
    public static readonly MyMessage ErrorOccurred = new ErrorOccurredMessage();

    private sealed class ErrorOccurredMessage : MyMessage  // ❌ Private nested
    {
        public ErrorOccurredMessage()
            : base(1, "ErrorOccurred", MessageSeverity.Error, "Error occurred")
        {
        }
    }
}
```

**Why This Is Wrong**:
- Source generator can't discover private nested classes
- Manual static properties instead of generated ones
- Doesn't work with `[Message]` attribute
- Inconsistent with new source generator pattern

---

## ✅ NEW PATTERN (Use This)

```csharp
// ✅ Base class
public abstract class MyMessage : MessageTemplate<MessageSeverity>
{
    protected MyMessage(int id, string name, MessageSeverity severity, string message, string? code = null)
        : base(id, name, severity, message, code, "Domain", null, null)
    {
    }
}

// ✅ Collection base
[MessageCollection("MyMessages")]
public abstract class MyMessageCollectionBase : MessageCollectionBase<MyMessage>
{
}

// ✅ Public sealed message class
[Message("ErrorOccurred")]
public sealed class ErrorOccurredMessage : MyMessage
{
    public ErrorOccurredMessage()
        : base(1, "ErrorOccurred", MessageSeverity.Error, "Error occurred", "MY_001")
    {
    }
}
```

**Why This Is Correct**:
- Source generator discovers `[Message]` attributes
- Generates static properties automatically
- Clean separation of concerns
- Consistent pattern across codebase

---

## Checklist

When creating messages, verify:

- [ ] Base class extends `MessageTemplate<MessageSeverity>`
- [ ] Collection base extends `MessageCollectionBase<TMessage>`
- [ ] Collection base has `[MessageCollection("Name")]` attribute
- [ ] Message classes are `public sealed`
- [ ] Message classes have `[Message("Name")]` attribute
- [ ] Message IDs are unique within domain
- [ ] Message codes follow PREFIX_### pattern
- [ ] Message text is clear and parameterized where needed
- [ ] Severity is appropriate
- [ ] One message class per file
- [ ] XML documentation provided

---

## Common Mistakes

### ❌ Using Static Class with Private Nested

```csharp
// WRONG - This is the old pattern
public static class MyMessages
{
    public static readonly MyMessage Error = new ErrorMessage();

    private sealed class ErrorMessage : MyMessage { }  // Source generator can't find this!
}
```

### ❌ Missing [Message] Attribute

```csharp
// WRONG - Missing attribute
public sealed class ErrorMessage : MyMessage
{
    // Source generator won't discover this!
}
```

### ❌ Private Constructor

```csharp
// WRONG - Constructor must be public
[Message("Error")]
public sealed class ErrorMessage : MyMessage
{
    private ErrorMessage() { }  // Source generator needs public access!
}
```

### ❌ Abstract Message Class

```csharp
// WRONG - Message classes must be sealed
[Message("Error")]
public abstract class ErrorMessage : MyMessage { }
```

---

## Template Parameters

For scaffolding message creation:

```json
{
  "symbols": {
    "MessageName": {
      "type": "parameter",
      "datatype": "string",
      "replaces": "MyMessage",
      "description": "Name of the message (PascalCase)"
    },
    "MessageId": {
      "type": "parameter",
      "datatype": "int",
      "replaces": "1",
      "description": "Unique message ID within domain"
    },
    "MessageCode": {
      "type": "parameter",
      "datatype": "string",
      "replaces": "MSG_001",
      "description": "Message code (PREFIX_###)"
    },
    "MessageText": {
      "type": "parameter",
      "datatype": "string",
      "replaces": "Operation failed",
      "description": "Message text (can include {0}, {1} placeholders)"
    },
    "MessageSeverity": {
      "type": "parameter",
      "datatype": "choice",
      "choices": [
        { "choice": "Error" },
        { "choice": "Warning" },
        { "choice": "Information" },
        { "choice": "Debug" }
      ],
      "defaultValue": "Error",
      "description": "Message severity"
    },
    "DomainName": {
      "type": "parameter",
      "datatype": "string",
      "replaces": "DataCommands",
      "description": "Domain/category name"
    }
  }
}
```

---

## Summary

**Correct Message Pattern**:
1. ✅ Base class extending `MessageTemplate<TSeverity>`
2. ✅ Collection base with `[MessageCollection]` extending `MessageCollectionBase<T>`
3. ✅ Public sealed message classes with `[Message]` attribute
4. ✅ Source generator creates static properties

**Consistency Requirements**:
- Unique IDs within domain
- Consistent code prefixes (PREFIX_###)
- Clear, parameterized message text
- Appropriate severity levels
- One message per file

**Don't Use**:
- ❌ Private nested classes
- ❌ Static class with manual properties
- ❌ Missing `[Message]` attribute
- ❌ Private constructors

Follow this pattern for all message creation in FractalDataWorks Developer Kit!
