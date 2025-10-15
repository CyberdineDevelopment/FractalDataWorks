# FractalDataWorks Developer Kit - Template Documentation Index

**Created**: 2025-10-14
**Purpose**: Comprehensive documentation for creating FractalDataWorks project and item templates

---

## Overview

This directory contains detailed documentation for creating Visual Studio/Rider item and project templates for the FractalDataWorks Developer Kit architecture. Each document provides:

- **What is it?** - Purpose and role in the architecture
- **Components** - Required files, dependencies, and structure
- **Dependencies** - Project references and source generator setup
- **Patterns** - Common patterns and best practices
- **Examples** - Complete, working code examples
- **Template Parameters** - For wizard-driven scaffolding
- **Common Mistakes** - Anti-patterns to avoid
- **Checklist** - Verification steps

---

## Template Documentation

### 1. Service Template
**File**: [01-SERVICE-TEMPLATE.md](./01-SERVICE-TEMPLATE.md)
**Size**: 34 KB
**Type**: Project Template

**What You'll Learn**:
- What makes up a Service vs Connection vs Command
- Two-project pattern (Abstractions + Implementation)
- ServiceType and ServiceTypeCollection for DI registration
- Source generator setup for Services, Messages, and Logging
- Factory and Provider patterns
- Complete example: Secret Manager service

**Key Insights**:
- Services are business logic/domain operations
- Services execute Commands
- ServiceTypes auto-register with DI
- Messages and Logging are source-generated

**Template Parameters**:
- ServiceName, DomainName
- IncludeMessages, IncludeConfiguration
- TargetFramework

---

### 2. Connection Template
**File**: [02-CONNECTION-TEMPLATE.md](./02-CONNECTION-TEMPLATE.md)
**Size**: 31 KB
**Type**: Project Template

**What You'll Learn**:
- What is a Connection (protocol/transport layer)
- Connection<TTranslator> inverted architecture pattern
- IConnectionCommand domain-specific representation
- Connection vs Service distinction
- State management and transactions
- Complete example: HTTP connection with REST and GraphQL translators

**Key Insights**:
- Connections handle protocol (HTTP, TCP, File I/O)
- Connections OWN their translators (inverted pattern)
- Translators convert DataCommands → ConnectionCommands
- One physical connection can support multiple command languages

**Template Parameters**:
- ConnectionType, ConnectionCategory
- IncludeStateMachine, IncludeTransactions
- IncludeQueryTranslator

---

### 3. DataCommand Template
**File**: [03-DATACOMMAND-TEMPLATE.md](./03-DATACOMMAND-TEMPLATE.md)
**Size**: 37 KB
**Type**: Item Template (adding new command to Commands.Data)

**What You'll Learn**:
- 3-level generic hierarchy (IDataCommand, IDataCommand<TResult>, IDataCommand<TResult, TInput>)
- Zero-boxing type safety
- Expression system (Filter, Projection, Ordering, Paging)
- FilterOperators TypeCollection (eliminates switch statements)
- DataCommandCategory assignment
- Complete examples: QueryCommand, UpsertCommand, BulkInsertCommand, MergeCommand

**Key Insights**:
- Commands are universal (work across SQL/REST/File/GraphQL)
- Generics eliminate casting and provide IntelliSense
- TypeCollection enables extensibility
- Operators are objects with behavior, not enum values

**Template Parameters**:
- CommandName, ResultType, InputType
- Category, UniqueId
- HasFilter, HasProjection, HasOrdering, HasPaging

---

### 4. Translator Template
**File**: [04-TRANSLATOR-TEMPLATE.md](./04-TRANSLATOR-TEMPLATE.md)
**Size**: ~35 KB
**Type**: Item Template (adding translator to Connection)

**What You'll Learn**:
- IDataCommandTranslator interface
- Visitor pattern for type-safe dispatch (no switch statements)
- Expression translation (FilterExpression → SQL WHERE / OData $filter)
- FilterOperators integration (operators know their own SQL/OData)
- Hybrid registration (runtime + compile-time)
- Complete examples: TSqlTranslator, RestTranslator

**Key Insights**:
- Translators live WITH connections (Translators/ subfolder)
- Connections register translators at runtime
- Visitor pattern avoids switch statements
- Operators encapsulate SQL/OData logic

**Template Parameters**:
- TranslatorName, DomainCategory
- TargetFramework
- IncludeExpressionVisitors

---

### 5. FilterOperator Template
**File**: [05-FILTEROPERATOR-ITEM-TEMPLATE.md](./05-FILTEROPERATOR-ITEM-TEMPLATE.md)
**Size**: 42 KB
**Type**: Item Template (adding operator to FilterOperators)

**What You'll Learn**:
- FilterOperatorBase architecture
- SQL and OData formatting
- FormatSqlParameter() and FormatODataValue()
- TypeOption for automatic discovery
- Complete examples: Equal, Contains, StartsWith, IsNull, In, GreaterThan

**Key Insights**:
- Operators are objects, not enums
- Each operator knows its own SQL, OData, parameter formatting
- No switch statements needed
- TypeCollection provides extensibility

**Template Parameters**:
- OperatorName, OperatorId
- SqlOperator, ODataOperator
- RequiresValue, HasCustomParameterFormat

---

### 6. Message Pattern Guide
**File**: [06-MESSAGE-PATTERN.md](./06-MESSAGE-PATTERN.md)
**Size**: ~25 KB
**Type**: Pattern Guide (not a template)

**What You'll Learn**:
- **CORRECT** message pattern (NEW)
- Why old pattern is wrong
- Message consistency requirements (IDs, codes, format)
- Source-generated message collections
- MessageCollectionBase with [MessageCollection] attribute
- Public sealed classes with [Message] attribute

**Key Insights**:
- Messages must be public sealed with [Message] attribute (not private nested!)
- Collection base gets [MessageCollection] attribute
- Source generator creates static properties automatically
- Message IDs should follow domain ranges
- Message codes follow PREFIX_### pattern

**Critical Requirements**:
- Public constructor (not private!)
- Sealed class (not abstract)
- [Message("Name")] attribute required
- One message per file
- Consistent ID ranges and code prefixes

---

## Architecture Decision Records

### Why Separate Documentation for Each Template?

1. **Clarity**: Each template solves a different problem
2. **Searchability**: Easier to find specific guidance
3. **Maintainability**: Update one without affecting others
4. **Complexity**: Each is 30-40KB of detailed content

### Why This Organization?

```
discussions/templates/
├── INDEX.md                          ← You are here
├── 01-SERVICE-TEMPLATE.md            ← Project template
├── 02-CONNECTION-TEMPLATE.md         ← Project template
├── 03-DATACOMMAND-TEMPLATE.md        ← Item template
├── 04-TRANSLATOR-TEMPLATE.md         ← Item template
├── 05-FILTEROPERATOR-ITEM-TEMPLATE.md ← Item template
```

**Rationale**:
- Numbered for reading order
- Service → Connection → Command → Translator → Operator (dependency order)
- Project templates first, then item templates
- Descriptive names for discoverability

---

## Quick Reference

### When to Use Each Template

| Template | Use When... | Example |
|----------|------------|---------|
| **Service** | Building business logic, domain operations | Authentication, SecretManager, DataGateway |
| **Connection** | Implementing protocol/transport layer | MsSql, PostgreSql, Redis, RabbitMQ, HttpClient |
| **DataCommand** | Adding universal data operation | UpsertCommand, MergeCommand, BulkUpdateCommand |
| **Translator** | Adding command language to connection | TSqlTranslator, RestTranslator, GraphQLTranslator |
| **FilterOperator** | Adding filter operation | Between, NotIn, RegexMatch, FullTextSearch |

### Dependency Flow

```
Service
  ↓ executes
Command
  ↓ translates via
Translator (lives in Connection)
  ↓ produces
ConnectionCommand
  ↓ executes on
Connection
  ↓ returns
Result
```

### Key Architectural Patterns

1. **TypeCollections** - Extensible, type-safe collections (replaces enums)
2. **ServiceTypeCollection** - Auto-register services with DI
3. **Railway-Oriented Programming** - Results instead of exceptions
4. **Source Generation** - Messages, Logging, TypeCollections
5. **Inverted Translator** - Connections own translators
6. **Visitor Pattern** - Type-safe dispatch without switch statements
7. **3-Level Generics** - Zero-boxing type safety (IDataCommand hierarchy)

---

## Understanding the Architecture

### The Big Picture

```
┌─────────────┐
│   Service   │ Business Logic
└──────┬──────┘
       │ executes
       ↓
┌─────────────┐
│   Command   │ Universal Operation (DataCommand)
└──────┬──────┘
       │ translates
       ↓
┌──────────────┐
│  Translator  │ Domain Conversion (SQL, REST, etc.)
│ (in Connection)
└──────┬───────┘
       │ produces
       ↓
┌──────────────────┐
│ ConnectionCommand│ Domain-Specific (SQL string, HTTP request)
└──────┬───────────┘
       │ executes on
       ↓
┌─────────────┐
│ Connection  │ Protocol/Transport
└─────────────┘
```

### What Makes a Service Different from a Connection?

| Aspect | Service | Connection |
|--------|---------|------------|
| **Purpose** | Business logic, domain operations | Protocol/transport layer |
| **Examples** | Authentication, SecretManager | MsSql, Redis, HttpClient |
| **Executes** | Commands (IGenericCommand) | ConnectionCommands (IConnectionCommand) |
| **Lifetime** | Singleton/Scoped/Transient | Usually Scoped |
| **State** | Stateless (mostly) | Stateful (connection pool, state machine) |
| **Dependencies** | Other services, connections | Low-level libraries (Npgsql, StackExchange.Redis) |

### What Makes a Command Different from a DataCommand?

| Aspect | IGenericCommand | IDataCommand |
|--------|-----------------|--------------|
| **Scope** | Generic, any operation | Data operations only |
| **Examples** | SendEmail, GenerateReport | Query, Insert, Update, Delete |
| **Translation** | Service-specific | Universal → domain-specific |
| **Type Safety** | Object-based | 3-level generics (zero boxing) |
| **Expressions** | N/A | Filter, Projection, Ordering, Paging |

---

## Using These Documents to Create Templates

### Visual Studio Item Template (.template.config/template.json)

Each document provides **Template Parameters** section with:
- Parameter name
- Data type (string, bool, choice)
- Description
- Replacement tokens
- Default values

**Example** (from 05-FILTEROPERATOR-ITEM-TEMPLATE.md):

```json
{
  "symbols": {
    "OperatorName": {
      "type": "parameter",
      "datatype": "string",
      "replaces": "MyOperator",
      "description": "Name of the filter operator",
      "defaultValue": "MyOperator"
    },
    "OperatorId": {
      "type": "parameter",
      "datatype": "int",
      "replaces": "999",
      "description": "Unique ID (100-999 range)",
      "defaultValue": "100"
    }
  }
}
```

### Rider File Template

Each document provides complete code examples that can be converted to Rider templates using `$PARAMETER$` syntax.

### dotnet new Template

Compatible with `dotnet new` template system using standard `template.json` format.

---

## Common Patterns Across All Templates

### 1. Source Generator Setup

**Always** use this pattern for source generators:

```xml
<!-- Collections Source Generator -->
<ProjectReference Include="..\FractalDataWorks.Collections.SourceGenerators\FractalDataWorks.Collections.SourceGenerators.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false"
                  PrivateAssets="all" />

<!-- Messages Source Generator - REQUIRED if using [MessageCollection] or [Message] -->
<ProjectReference Include="..\FractalDataWorks.Messages.SourceGenerators\FractalDataWorks.Messages.SourceGenerators.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false"
                  PrivateAssets="all" />

<!-- EnhancedEnums Source Generator - REQUIRED if using EnhancedEnum -->
<ProjectReference Include="..\FractalDataWorks.EnhancedEnums.SourceGenerators\FractalDataWorks.EnhancedEnums.SourceGenerators.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false"
                  PrivateAssets="all" />
```

**Critical**:
- `OutputItemType="Analyzer"` tells MSBuild to run the generator at compile time
- `ReferenceOutputAssembly="false"` means don't reference the DLL
- `PrivateAssets="all"` prevents leaking to package consumers
- **Without these**, attributes like `[TypeCollection]`, `[MessageCollection]`, `[Message]` won't generate code!

### 2. TypeCollection Pattern

```csharp
[TypeCollection(typeof(BaseClass), typeof(IInterface), typeof(CollectionClass))]
public abstract partial class CollectionClass : TypeCollectionBase<BaseClass, IInterface>
{
    // Source generator creates static properties
}
```

### 3. TypeOption Pattern

```csharp
[TypeOption(typeof(CollectionClass), "OptionName")]
public sealed class ConcreteImplementation : BaseClass
{
    // Implementation
}
```

### 4. ServiceTypeOption Pattern

```csharp
[ServiceTypeOption(typeof(ServiceTypeCollection), "ServiceName")]
public sealed class ServiceType : ServiceTypeBase<IService, ServiceConfiguration>
{
    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IService, ServiceImplementation>();
    }
}
```

### 5. Results Pattern (Railway-Oriented Programming)

```csharp
// WRONG
public void DoSomething()
{
    if (condition) throw new Exception("Error");
}

// CORRECT
public IGenericResult DoSomething()
{
    if (!condition)
        return GenericResult.Failure(SomeMessages.ErrorMessage);

    return GenericResult.Success();
}
```

### 6. Source-Generated Logging

```csharp
public partial class MyService
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Operation {OperationName} completed successfully")]
    private partial void LogOperationCompleted(string operationName);
}
```

### 7. Message Collections

> **⚠️ IMPORTANT**: The pattern below is OUTDATED. See [06-MESSAGE-PATTERN.md](./06-MESSAGE-PATTERN.md) for the CORRECT pattern.

**CORRECT Pattern** (see full details in 06-MESSAGE-PATTERN.md):

```csharp
// Step 1: Base class
public abstract class MyServiceMessage : MessageTemplate<MessageSeverity>
{
    protected MyServiceMessage(int id, string name, MessageSeverity severity, string message, string? code = null)
        : base(id, name, severity, message, code, "MyService", null, null)
    {
    }
}

// Step 2: Collection base
[MessageCollection("MyServiceMessages")]
public abstract class MyServiceMessageCollectionBase : MessageCollectionBase<MyServiceMessage>
{
}

// Step 3: Public sealed message classes with [Message] attribute
[Message("OperationFailed")]
public sealed class OperationFailedMessage : MyServiceMessage
{
    public OperationFailedMessage()
        : base(1, "OperationFailed", MessageSeverity.Error, "Operation failed: {0}", "MYSVC_001")
    {
    }
}

// Step 4: Source generator creates static properties automatically:
// public static partial class MyServiceMessages
// {
//     public static OperationFailedMessage OperationFailed => new();
// }
```

**Key Requirements**:
- Message classes must be `public sealed` with `[Message]` attribute
- Collection base must have `[MessageCollection]` attribute
- Source generator discovers and creates static properties
- See **[06-MESSAGE-PATTERN.md](./06-MESSAGE-PATTERN.md)** for complete details

---

## CLAUDE.md Compliance

All templates enforce these rules from CLAUDE.md:

### ❌ Never Do

1. **Enums** - Use TypeCollection or EnhancedEnum
2. **Switch statements** - Use properties or visitor pattern
3. **Manual service registration** - Use ServiceTypeCollection
4. **ArgumentNullException** - Return Result with message
5. **Async suffix** - Method names shouldn't end in "Async"
6. **Core suffix pattern** - Avoid `ValidateCore` called by `Validate`
7. **String literals** - Use `nameof` and `typeof`
8. **Manual logging** - Use source-generated `[LoggerMessage]`
9. **Exceptions for anticipated conditions** - Use Results

### ✅ Always Do

1. **Railway-Oriented Programming** - Return `IGenericResult` or `IGenericResult<T>`
2. **TypeCollections** - For extensible fixed sets
3. **EnhancedEnums** - For simple fixed sets (2-5 values)
4. **Properties instead of switch** - Operators know their own SQL, OData
5. **ServiceTypeCollection** - For DI registration
6. **Source-generated logging** - With `[LoggerMessage]`
7. **Messages from MessageTemplate** - Structured error messages
8. **Constructor-based properties** - In TypeCollections (not abstract)

### Target Frameworks

- **Abstractions**: `netstandard2.0` ONLY
- **Collections/ServiceTypes**: Multi-target `netstandard2.0;net10.0`
- **SourceGenerators**: `netstandard2.0` ONLY
- **Implementations**: `net10.0`

---

## Next Steps

### For Template Authors

1. **Read** the relevant template documentation
2. **Study** the complete examples provided
3. **Create** `.template.config/template.json` using Template Parameters section
4. **Implement** template files with replacement tokens
5. **Test** template generation
6. **Validate** against checklist in each document

### For Developers Using Templates

1. **Review** this INDEX to understand architecture
2. **Select** appropriate template for your need
3. **Read** the specific template documentation
4. **Use** Visual Studio/Rider "New Item" or `dotnet new`
5. **Follow** the checklist at end of each document
6. **Refer** to Common Mistakes section

---

## Contributing

When adding new template documentation:

1. Follow the structure of existing documents
2. Include complete, working code examples
3. Provide Template Parameters for scaffolding
4. Document common mistakes
5. Add checklist for validation
6. Update this INDEX.md with new entry

---

## References

### Internal Documentation
- [ARCHITECTURE_SUMMARY.md](../../ARCHITECTURE_SUMMARY.md) - Overall architecture
- [CONTINUATION_GUIDE.md](../../CONTINUATION_GUIDE.md) - Implementation guide
- [INVERTED_TRANSLATOR_ARCHITECTURE.md](../../INVERTED_TRANSLATOR_ARCHITECTURE.md) - Translator pattern
- [CLAUDE.md](../../CLAUDE.md) - Development standards

### External Resources
- [.NET Template Documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/custom-templates)
- [Visual Studio Item Templates](https://learn.microsoft.com/en-us/visualstudio/ide/creating-project-and-item-templates)
- [Rider File Templates](https://www.jetbrains.com/help/rider/Using_File_and_Code_Templates.html)

---

## Summary

This template documentation provides **everything needed** to:

✅ Understand FractalDataWorks architecture
✅ Distinguish between Services, Connections, Commands, Translators, Operators
✅ Create Visual Studio/Rider item and project templates
✅ Follow architectural patterns and best practices
✅ Avoid common mistakes
✅ Ensure CLAUDE.md compliance

**Total Documentation**: ~180 KB across 6 files
**Coverage**: Complete architecture from Service → Connection → Command → Translator → Operator

**Ready to accelerate FractalDataWorks development!** 🚀
