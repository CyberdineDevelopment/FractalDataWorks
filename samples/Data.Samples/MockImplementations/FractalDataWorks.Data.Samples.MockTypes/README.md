# FractalDataWorks Data Architecture - TypeCollection Pattern Demo

This sample demonstrates the **TypeCollection pattern** used throughout the FractalDataWorks Data Architecture for configuration-driven, metadata-first data access.

## What This Sample Shows

This project contains **7 mock type definitions** that demonstrate all 6 data architecture TypeCollections:

1. **PathTypes** - Where data lives (databases, APIs, files)
2. **ContainerTypes** - What holds data (tables, views, collections)
3. **TranslatorTypes** - How to query data (SQL, OData, GraphQL)
4. **FormatTypes** - How data is serialized (JSON, XML, CSV)
5. **DataTypeConverterTypes** - Type mappings (SQL int → CLR Int32)
6. **DataTransformerTypes** - ETL operations (aggregate, filter, enrich)

## Architecture Principles Demonstrated

### 1. Configuration-First Design
Instead of hardcoding type names in code, configuration files reference types by string name:

```json
{
  "DataConnection": {
    "PathType": "SqlDatabase",      // Resolved at runtime
    "ContainerType": "SqlTable",    // No coupling to implementation classes
    "TranslatorType": "TSqlQuery",  // TypeCollections handle lookup
    "FormatType": "Json"
  }
}
```

### 2. No Heuristics
Field roles (Identity, Attribute, Measure) are **explicitly specified** in configuration, never inferred from column names or data types.

### 3. TypeCollection Pattern
The `[TypeOption]` attribute marks types for automatic discovery by the TypeCollectionGenerator source generator:

```csharp
[TypeOption(typeof(PathTypes), "SqlDatabase")]
public sealed class MockSqlPathType : PathTypeBase
{
    // Metadata only - describes what "SQL database path" means
    // Actual path implementation lives in PathBase-derived classes
}
```

### 4. Metadata vs. Implementation Separation
This sample demonstrates the **metadata layer**:
- **`PathTypeBase`** - Metadata about path types (used by TypeCollection)
- **`PathBase`** - Actual path implementation (e.g., DatabasePath with connection logic)

Production code has both layers; this sample shows only the metadata layer.

### 5. Cross-Assembly Discovery
TypeCollections automatically discover types across assemblies:
- `DataTypeConverterTypes` uses `RestrictToCurrentCompilation = false`
- Converters from SQL, JSON, XML assemblies all discovered together
- Extensible without modifying core library

## Mock Types Included

| Type | Collection | Domain | Key Features |
|------|-----------|--------|--------------|
| `MockSqlPathType` | PathTypes | Sql | Database object paths |
| `MockRestPathType` | PathTypes | Rest | API endpoint URLs |
| `MockTableContainerType` | ContainerTypes | - | Schema discovery support |
| `MockTSqlTranslatorType` | TranslatorTypes | Sql | Universal → T-SQL translation |
| `MockJsonFormatType` | FormatTypes | - | Streaming, MIME type |
| `MockSqlInt32ConverterType` | DataTypeConverterTypes | - | SQL int → CLR Int32 |
| `MockAggregateTransformerType` | DataTransformerTypes | - | ETL aggregation |

## Running the Sample

```bash
cd samples/Data.Samples/MockImplementations/FractalDataWorks.Data.Samples.MockTypes
dotnet run
```

**Expected Output:**
Currently shows 0 registered types because the TypeCollectionGenerator source generator only discovers types within the compilation that references the generator. This demonstrates the **metadata structure** and **pattern usage** - production implementations would be discovered when deployed in real projects.

## How TypeCollections Work

### Step 1: Define the Type Metadata
```csharp
[TypeOption(typeof(PathTypes), "SqlDatabase")]
public sealed class MockSqlPathType : PathTypeBase
{
    public MockSqlPathType()
        : base(id: 1, name: "SqlDatabase",
               displayName: "SQL Server Database Path",
               description: "Path to SQL Server database objects",
               domain: "Sql")
    { }
}
```

### Step 2: Generator Discovers at Compile Time
The `TypeCollectionGenerator` scans assemblies for `[TypeOption]` attributes and generates registration code:

```csharp
// Generated code (simplified)
public sealed partial class PathTypes : TypeCollectionBase<PathTypeBase, IPathType>
{
    static PathTypes()
    {
        RegisterType(new MockSqlPathType());
        RegisterType(new MockRestPathType());
        // ... other discovered types
    }
}
```

### Step 3: Runtime Resolution
Configuration strings resolve to type metadata:

```csharp
// Configuration: "PathType": "SqlDatabase"
var pathType = PathTypes.Get("SqlDatabase");
Console.WriteLine(pathType.DisplayName); // "SQL Server Database Path"
Console.WriteLine(pathType.Domain);      // "Sql"
```

## Key TypeCollection Features

### Zero Generic Parameters
All base types use 0 generic parameters - nesting is achieved through interface composition:

```csharp
public interface IArrayFieldType : IFieldType
{
    IFieldType ElementType { get; } // Can be Simple, Array, or Object
}
```

### Domain Classification
Types are grouped by domain (Sql, Rest, File, GraphQL) for architectural coherence:
- SQL path → SQL container → SQL translator (coherent pipeline)
- REST path → REST container → OData translator (coherent pipeline)

### Rich Metadata
Types carry compile-time safe metadata:
- **FormatTypes**: MimeType, IsBinary, SupportsStreaming
- **ContainerTypes**: SupportsSchemaDiscovery
- **TransformerTypes**: SupportsStreaming
- **ConverterTypes**: SourceTypeName, TargetClrType

## Integration with Field Roles

The architecture uses explicit **FieldRole** enumeration:

```csharp
public enum FieldRole
{
    Identity,   // Primary keys, unique identifiers
    Attribute,  // Descriptive/dimensional data
    Measure     // Numeric/aggregatable facts
}
```

Transformers operate on specific roles:
- **Aggregate Transformer**: Operates on `FieldRole.Measure` fields only
- **Filter Transformer**: Can use `FieldRole.Identity` or `FieldRole.Attribute` for criteria
- **Enrichment Transformer**: Adds new `FieldRole.Attribute` fields

## Production Usage

In production, you would:

1. **Create actual implementations** (PathBase-derived, TranslatorBase-derived, etc.)
2. **Reference the source generator** in your project
3. **Mark types with `[TypeOption]`** attributes
4. **Configure via appsettings.json** using type names
5. **Let TypeCollections resolve** types at runtime

Example production structure:
```
FractalDataWorks.Data.Sql/
  ├── SqlDatabasePathType.cs       // Metadata (marked with [TypeOption])
  ├── SqlDatabasePath.cs           // Implementation (PathBase-derived)
  ├── TSqlQueryTranslatorType.cs   // Metadata
  ├── TSqlQueryTranslator.cs       // Implementation
  └── appsettings.json             // References types by name
```

## Architecture Benefits

1. **No Switch Statements**: Type resolution via TypeCollections, not control flow
2. **Extensible**: New types discovered without modifying core library
3. **Type Safe**: Compile-time metadata properties, not runtime reflection
4. **Testable**: Mock types for testing without database connections
5. **Configuration-First**: Runtime behavior driven by appsettings.json
6. **Zero Allocation**: Static formatters with pooling (ArrayPool, ObjectPool)

## See Also

- **`FractalDataWorks.Data.Abstractions`** - Core interfaces and base types
- **`FractalDataWorks.Collections.SourceGenerators`** - TypeCollectionGenerator implementation
- **Phase 1 Implementation Summary** - `docs/Phase1-Implementation-Summary.md`

## Learn More

Each mock type file contains detailed XML documentation explaining:
- What architectural pattern it demonstrates
- How it integrates with the overall architecture
- Production usage examples
- Configuration patterns

Read the source files for comprehensive explanations of the TypeCollection pattern.
