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
The `[TypeOption]` attribute marks types for automatic discovery by the TypeCollectionGenerator source generator.

**Single-Class Pattern** - Each type contains both metadata AND implementation:

```csharp
[TypeOption(typeof(DataTypeConverterTypes), "SqlInt32")]
public sealed class MockSqlInt32Converter : DataTypeConverterBase
{
    // Metadata properties
    public override string SourceTypeName => "int";
    public override Type TargetClrType => typeof(int);

    // Implementation methods
    public override object? Convert(object? value) { /* ... */ }
    public override object? ConvertBack(object? clrValue) { /* ... */ }
}
```

**Why Single-Class Pattern?**
- ✅ Simpler - one class, one responsibility
- ✅ Direct access - `GetByName()` returns usable instance immediately
- ✅ No duplication - metadata and implementation in same place
- ✅ Clear ownership - TypeCollection manages instances

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
| `MockSqlInt32Converter` | DataTypeConverterTypes | - | SQL int → CLR Int32 (single-class pattern) |
| `MockAggregateTransformerType` | DataTransformerTypes | - | ETL aggregation |

## Running the Sample

```bash
cd samples/Data.Samples/MockImplementations/FractalDataWorks.Data.Samples.MockTypes
dotnet run
```

**Expected Output:**
Currently shows 0 registered types because the TypeCollectionGenerator source generator only discovers types within the compilation that references the generator. This demonstrates the **metadata structure** and **pattern usage** - production implementations would be discovered when deployed in real projects.

## How TypeCollections Work

### Step 1: Define the Type with Attribute
```csharp
[TypeOption(typeof(DataTypeConverterTypes), "SqlInt32")]
public sealed class MockSqlInt32Converter : DataTypeConverterBase
{
    public MockSqlInt32Converter()
        : base(id: 1, name: "SqlInt32")
    { }

    public override string SourceTypeName => "int";
    public override Type TargetClrType => typeof(int);

    public override object? Convert(object? value)
    {
        if (value is null or DBNull) return null;
        return System.Convert.ToInt32(value);
    }

    public override object? ConvertBack(object? clrValue) => clrValue;
}
```

### Step 2: Generator Discovers at Compile Time
The `TypeCollectionGenerator` scans assemblies for `[TypeOption]` attributes and generates registration code:

```csharp
// Generated code (simplified)
public sealed partial class DataTypeConverterTypes : TypeCollectionBase<DataTypeConverterBase, IDataTypeConverter>
{
    static DataTypeConverterTypes()
    {
        RegisterType(() => new MockSqlInt32Converter());
        // ... other discovered converters
    }
}
```

### Step 3: Runtime Resolution
Configuration strings resolve to actual converter instances:

```csharp
// Configuration: "ConverterTypeName": "SqlInt32"
var converter = DataTypeConverterTypes.GetByName("SqlInt32");
var clrValue = converter.Convert(dbValue);  // Direct call!
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
  ├── Paths/
  │   └── DatabasePath.cs          // Single class: metadata + implementation
  ├── Translators/
  │   ├── TSqlQueryTranslator.cs   // Single class: metadata + implementation
  │   └── TSqlSprocTranslator.cs   // Single class: metadata + implementation
  ├── Converters/
  │   ├── SqlInt32Converter.cs     // Single class with [TypeOption]
  │   └── SqlStringConverter.cs    // Single class with [TypeOption]
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
