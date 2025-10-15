# FilterOperator Item Template Documentation

## 1. What is a FilterOperator?

FilterOperators replace traditional enum-based operator systems with **behavior-rich classes** that eliminate switch statements and enable extensibility.

**Key Benefits:**
- **No Switch Statements**: Each operator knows its own SQL and OData representations
- **Type-Safe**: Compile-time checking via TypeCollection pattern
- **Extensible**: Add new operators without modifying existing code
- **Behavior-Rich**: Operators handle their own parameter formatting and value translation

**Example Comparison:**

```csharp
// OLD WAY: Enum + Switch Statements
enum FilterOperator { Equal, NotEqual, Contains }

string GetSqlOperator(FilterOperator op) {
    switch (op) {  // Switch statement required!
        case FilterOperator.Equal: return "=";
        case FilterOperator.NotEqual: return "<>";
        case FilterOperator.Contains: return "LIKE";
        default: throw new ArgumentException();
    }
}

// NEW WAY: Behavior-Rich Classes
var condition = new FilterCondition {
    Operator = FilterOperators.Equal  // Type-safe!
};
var sql = condition.Operator.SqlOperator;  // Direct property access - no switch!
```

## 2. FilterOperatorBase Architecture

FilterOperatorBase is the abstract base class all operators inherit from. It defines the contract and provides default implementations.

**Source:** `src/FractalDataWorks.Commands.Data.Abstractions/Operators/FilterOperatorBase.cs`

```csharp
public abstract class FilterOperatorBase
{
    /// <summary>
    /// Constructor parameters define operator metadata.
    /// Properties are set in constructor so TypeCollection source generator
    /// can read them without instantiation.
    /// </summary>
    protected FilterOperatorBase(
        int id,              // Unique identifier (e.g., 1, 2, 3...)
        string name,         // Name matching [TypeOption] attribute
        string sqlOperator,  // SQL representation (e.g., "=", "LIKE", "IS NULL")
        string odataOperator,// OData representation (e.g., "eq", "contains")
        bool requiresValue)  // Whether operator needs a value parameter
    {
        Id = id;
        Name = name;
        SqlOperator = sqlOperator;
        ODataOperator = odataOperator;
        RequiresValue = requiresValue;
    }

    // Direct property access - no switch statements!
    public int Id { get; }
    public string Name { get; }
    public string SqlOperator { get; }
    public string ODataOperator { get; }
    public bool RequiresValue { get; }

    /// <summary>
    /// Formats the parameter placeholder for SQL.
    /// VIRTUAL: Override for special behavior (e.g., LIKE wildcards).
    /// </summary>
    /// <param name="paramName">The parameter name.</param>
    /// <returns>Formatted SQL parameter (e.g., "@paramName", "'%' + @paramName + '%'").</returns>
    public virtual string FormatSqlParameter(string paramName)
        => $"@{paramName}";

    /// <summary>
    /// Formats the value for OData query strings.
    /// ABSTRACT: Must implement type-specific formatting.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>Formatted OData value (e.g., "'text'", "123", "true").</returns>
    public abstract string FormatODataValue(object? value);
}
```

**Key Design Points:**

1. **Constructor-Based Initialization**: Properties set in constructor allow TypeCollection source generator to read metadata without creating instances
2. **Virtual FormatSqlParameter()**: Default implementation handles simple `@paramName`, override for complex patterns (wildcards, ranges)
3. **Abstract FormatODataValue()**: Each operator must implement type-specific OData formatting
4. **RequiresValue Flag**: Some operators (IS NULL, IS NOT NULL) don't need values

## 3. Creating a New Operator - Step-by-Step

### Step 1: Create Class File

Create a new `.cs` file in `src/FractalDataWorks.Commands.Data/Operators/`:
- File name should match operator name: `{OperatorName}Operator.cs`
- Example: `EqualOperator.cs`, `ContainsOperator.cs`

### Step 2: Add Required Using Statements

```csharp
using System;  // If using DateTime, Guid, etc.
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;
```

### Step 3: Add TypeOption Attribute

The `[TypeOption]` attribute registers the operator with the TypeCollection:

```csharp
[TypeOption(typeof(FilterOperators), "OperatorName")]
public sealed class OperatorNameOperator : FilterOperatorBase
```

**Critical**: The string `"OperatorName"` must:
- Match the constructor `name` parameter exactly
- Become the property name on `FilterOperators` class (e.g., `FilterOperators.OperatorName`)
- Follow PascalCase naming convention

### Step 4: Implement Constructor

```csharp
public OperatorNameOperator()
    : base(
        id: [unique_number],           // Choose next available ID
        name: "OperatorName",          // Must match [TypeOption]
        sqlOperator: "[SQL_OP]",       // SQL syntax (e.g., "=", "LIKE", "IS NULL")
        odataOperator: "[ODATA_OP]",   // OData syntax (e.g., "eq", "contains")
        requiresValue: [true/false])   // Does operator need a value?
{
}
```

**ID Assignment**: Use sequential IDs. Check existing operators for next available number.

### Step 5: Override FormatSqlParameter() (If Needed)

Only override if you need custom SQL parameter formatting:

```csharp
/// <summary>
/// Formats SQL parameter with [custom behavior description].
/// </summary>
public override string FormatSqlParameter(string paramName)
    => "[custom format string]";
```

**Common Override Scenarios:**
- LIKE wildcards (Contains, StartsWith, EndsWith)
- BETWEEN ranges (requires two parameters)
- Empty return for operators with no parameters (IS NULL)

### Step 6: Implement FormatODataValue()

**Required** for all operators. Handle type-specific OData formatting:

```csharp
/// <inheritdoc/>
public override string FormatODataValue(object? value)
{
    if (value == null)
        return "null";

    return value switch
    {
        string str => $"'{str.Replace("'", "''")}'",  // Escape quotes!
        int or long or short or byte => value.ToString()!,
        decimal or double or float => value.ToString()!,
        bool b => b.ToString().ToLowerInvariant(),
        DateTime dt => $"datetime'{dt:yyyy-MM-ddTHH:mm:ss}'",
        DateTimeOffset dto => $"datetimeoffset'{dto:yyyy-MM-ddTHH:mm:sszzz}'",
        Guid guid => $"guid'{guid}'",
        _ => $"'{value}'"  // Fallback
    };
}
```

### Step 7: Add XML Documentation

Document the operator with clear XML comments:

```csharp
/// <summary>
/// [Operator description] operator ([SQL syntax], [OData syntax]).
/// [Special behavior notes if any].
/// </summary>
/// <remarks>
/// [Optional usage examples and additional context]
/// </remarks>
```

## 4. Complete Examples

### Example 1: Simple Operator (Equal)

**Source:** `src/FractalDataWorks.Commands.Data/Operators/EqualOperator.cs`

```csharp
using System;
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Equal operator (=, eq).
/// No switch statements needed - operator knows its own representations!
/// </summary>
/// <remarks>
/// <para>
/// Usage example:
/// <code>
/// var condition = new FilterCondition {
///     PropertyName = "Status",
///     Operator = FilterOperators.Equal,  // Type-safe, no magic strings!
///     Value = "Active"
/// };
///
/// // Direct property access - no switch!
/// var sqlCondition = $"[{condition.PropertyName}] {condition.Operator.SqlOperator} @{condition.PropertyName}";
/// // Result: "[Status] = @Status"
///
/// var odataFilter = $"{condition.PropertyName} {condition.Operator.ODataOperator} {condition.Operator.FormatODataValue(condition.Value)}";
/// // Result: "Status eq 'Active'"
/// </code>
/// </para>
/// </remarks>
[TypeOption(typeof(FilterOperators), "Equal")]
public sealed class EqualOperator : FilterOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EqualOperator"/> class.
    /// </summary>
    public EqualOperator()
        : base(
            id: 1,
            name: "Equal",
            sqlOperator: "=",
            odataOperator: "eq",
            requiresValue: true)
    {
    }

    /// <summary>
    /// Formats the value for OData query strings.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The formatted OData value string.</returns>
    public override string FormatODataValue(object? value)
    {
        if (value == null)
            return "null";

        return value switch
        {
            string str => $"'{str.Replace("'", "''")}'",  // Escape single quotes
            int or long or short or byte => value.ToString()!,
            decimal or double or float => value.ToString()!,
            bool b => b.ToString().ToLowerInvariant(),  // true/false (lowercase)
            DateTime dt => $"datetime'{dt:yyyy-MM-ddTHH:mm:ss}'",
            DateTimeOffset dto => $"datetimeoffset'{dto:yyyy-MM-ddTHH:mm:sszzz}'",
            Guid guid => $"guid'{guid}'",
            _ => $"'{value}'"  // Fallback: string format
        };
    }
}
```

**Key Points:**
- Uses default `FormatSqlParameter()` (no override needed)
- Comprehensive type handling in `FormatODataValue()`
- String escaping (`Replace("'", "''")`) prevents OData syntax errors

### Example 2: Complex Operator with Wildcards (Contains)

**Source:** `src/FractalDataWorks.Commands.Data/Operators/ContainsOperator.cs`

```csharp
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Contains operator (LIKE '%value%', contains).
/// Overrides FormatSqlParameter to add wildcards for SQL LIKE.
/// </summary>
[TypeOption(typeof(FilterOperators), "Contains")]
public sealed class ContainsOperator : FilterOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContainsOperator"/> class.
    /// </summary>
    public ContainsOperator()
        : base(
            id: 3,
            name: "Contains",
            sqlOperator: "LIKE",
            odataOperator: "contains",
            requiresValue: true)
    {
    }

    /// <summary>
    /// Formats SQL parameter with wildcards for LIKE pattern matching.
    /// </summary>
    public override string FormatSqlParameter(string paramName)
        => $"'%' + @{paramName} + '%'";  // Wraps value with SQL wildcards

    /// <inheritdoc/>
    public override string FormatODataValue(object? value)
    {
        // OData contains() function handles wildcards itself
        return value switch
        {
            string str => $"'{str.Replace("'", "''")}'",
            _ => $"'{value}'"
        };
    }
}
```

**Key Points:**
- **Overrides `FormatSqlParameter()`** to add SQL LIKE wildcards (`'%' + @param + '%'`)
- OData `contains()` function doesn't need wildcards
- SQL: `WHERE Name LIKE '%' + @Name + '%'`
- OData: `?$filter=contains(Name, 'value')`

### Example 3: Wildcard Variations

**StartsWith Operator:**

```csharp
/// <summary>
/// StartsWith operator (LIKE 'value%', startswith).
/// </summary>
[TypeOption(typeof(FilterOperators), "StartsWith")]
public sealed class StartsWithOperator : FilterOperatorBase
{
    public StartsWithOperator()
        : base(
            id: 4,
            name: "StartsWith",
            sqlOperator: "LIKE",
            odataOperator: "startswith",
            requiresValue: true)
    {
    }

    /// <summary>
    /// Formats SQL parameter with trailing wildcard.
    /// </summary>
    public override string FormatSqlParameter(string paramName)
        => $"@{paramName} + '%'";  // Trailing wildcard only

    public override string FormatODataValue(object? value)
    {
        return $"'{value?.ToString()?.Replace("'", "''")}'";
    }
}
```

**EndsWith Operator:**

```csharp
/// <summary>
/// EndsWith operator (LIKE '%value', endswith).
/// </summary>
[TypeOption(typeof(FilterOperators), "EndsWith")]
public sealed class EndsWithOperator : FilterOperatorBase
{
    public EndsWithOperator()
        : base(
            id: 5,
            name: "EndsWith",
            sqlOperator: "LIKE",
            odataOperator: "endswith",
            requiresValue: true)
    {
    }

    /// <summary>
    /// Formats SQL parameter with leading wildcard.
    /// </summary>
    public override string FormatSqlParameter(string paramName)
        => $"'%' + @{paramName}";  // Leading wildcard only

    public override string FormatODataValue(object? value)
    {
        return $"'{value?.ToString()?.Replace("'", "''")}'";
    }
}
```

**Wildcard Pattern Summary:**

| Operator   | SQL Parameter Format       | Matches         |
|------------|----------------------------|-----------------|
| Contains   | `'%' + @param + '%'`       | Anywhere        |
| StartsWith | `@param + '%'`             | Beginning       |
| EndsWith   | `'%' + @param`             | End             |

### Example 4: Null-Check Operator (No Value Required)

**Source:** `src/FractalDataWorks.Commands.Data/Operators/IsNullOperator.cs`

```csharp
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// IS NULL operator (IS NULL, eq null).
/// This operator does NOT require a value parameter.
/// </summary>
[TypeOption(typeof(FilterOperators), "IsNull")]
public sealed class IsNullOperator : FilterOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IsNullOperator"/> class.
    /// </summary>
    public IsNullOperator()
        : base(
            id: 10,
            name: "IsNull",
            sqlOperator: "IS NULL",
            odataOperator: "eq null",
            requiresValue: false)  // No value needed!
    {
    }

    /// <summary>
    /// Returns empty string since IS NULL doesn't use parameters.
    /// </summary>
    public override string FormatSqlParameter(string paramName)
        => string.Empty;

    /// <summary>
    /// Returns empty string since OData handles "eq null" without formatting.
    /// </summary>
    public override string FormatODataValue(object? value)
        => string.Empty;
}
```

**Key Points:**
- `requiresValue: false` in constructor
- Both formatting methods return `string.Empty`
- SQL: `WHERE Name IS NULL` (no parameter)
- OData: `?$filter=Name eq null` (no value formatting)

### Example 5: Multi-Value Operator (In)

**Source:** `src/FractalDataWorks.Commands.Data/Operators/InOperator.cs`

```csharp
using System.Collections;
using System.Linq;
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// IN operator (IN (...), in).
/// Value should be an IEnumerable of values.
/// </summary>
[TypeOption(typeof(FilterOperators), "In")]
public sealed class InOperator : FilterOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InOperator"/> class.
    /// </summary>
    public InOperator()
        : base(
            id: 12,
            name: "In",
            sqlOperator: "IN",
            odataOperator: "in",
            requiresValue: true)
    {
    }

    /// <summary>
    /// Formats SQL parameter for IN clause.
    /// Value should be enumerable - translator will expand to multiple parameters.
    /// </summary>
    public override string FormatSqlParameter(string paramName)
    {
        // Translator will expand this to (@param1, @param2, @param3, ...)
        return $"@{paramName}";
    }

    /// <inheritdoc/>
    public override string FormatODataValue(object? value)
    {
        if (value == null)
            return "()";

        if (value is IEnumerable enumerable and not string)
        {
            var values = enumerable.Cast<object>()
                .Select(v => v switch
                {
                    string str => $"'{str.Replace("'", "''")}'",
                    int or long or short or byte => v.ToString(),
                    decimal or double or float => v.ToString(),
                    _ => $"'{v}'"
                });

            return $"({string.Join(",", values)})";
        }

        return $"('{value}')";
    }
}
```

**Key Points:**
- Handles `IEnumerable` values
- SQL translator expands to multiple parameters
- OData formatting creates comma-separated list in parentheses
- SQL: `WHERE Status IN (@Status_0, @Status_1, @Status_2)`
- OData: `?$filter=Status in ('Active','Pending','Closed')`

### Example 6: Comparison Operator (GreaterThan)

**Source:** `src/FractalDataWorks.Commands.Data/Operators/GreaterThanOperator.cs`

```csharp
using System;
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Greater than operator (>, gt).
/// </summary>
[TypeOption(typeof(FilterOperators), "GreaterThan")]
public sealed class GreaterThanOperator : FilterOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GreaterThanOperator"/> class.
    /// </summary>
    public GreaterThanOperator()
        : base(
            id: 6,
            name: "GreaterThan",
            sqlOperator: ">",
            odataOperator: "gt",
            requiresValue: true)
    {
    }

    /// <inheritdoc/>
    public override string FormatODataValue(object? value)
    {
        if (value == null)
            return "null";

        return value switch
        {
            int or long or short or byte => value.ToString()!,
            decimal or double or float => value.ToString()!,
            DateTime dt => $"datetime'{dt:yyyy-MM-ddTHH:mm:ss}'",
            DateTimeOffset dto => $"datetimeoffset'{dto:yyyy-MM-ddTHH:mm:sszzz}'",
            _ => $"'{value}'"
        };
    }
}
```

**Key Points:**
- Focuses on numeric and date types
- No string escaping needed (comparison operators typically used with numbers/dates)
- Uses default `FormatSqlParameter()` implementation

## 5. Dependencies and Project References

### Project Reference

Add to your `.csproj` file (if creating a new project):

```xml
<ItemGroup>
  <ProjectReference Include="..\FractalDataWorks.Commands.Data.Abstractions\FractalDataWorks.Commands.Data.Abstractions.csproj" />
</ItemGroup>
```

### Required Using Statements

**Minimum Required:**

```csharp
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;
```

**Common Additional:**

```csharp
using System;                    // For DateTime, Guid, DateTimeOffset
using System.Collections;        // For IEnumerable (multi-value operators)
using System.Linq;              // For LINQ operations on collections
```

### Namespace Convention

All operator implementations should be in:

```csharp
namespace FractalDataWorks.Commands.Data;
```

## 6. TypeCollection Integration

### How TypeCollection Works

The TypeCollection pattern provides **compile-time type discovery** without reflection:

1. **Declaration**: `FilterOperators` class is marked with `[TypeCollection]` attribute
2. **Registration**: Each operator class marked with `[TypeOption(typeof(FilterOperators), "Name")]`
3. **Source Generation**: Build-time code generation creates static properties and methods
4. **Usage**: Type-safe access via `FilterOperators.OperatorName`

### FilterOperators TypeCollection Class

**Source:** `src/FractalDataWorks.Commands.Data.Abstractions/Operators/FilterOperators.cs`

```csharp
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// TypeCollection for filter operators.
/// Source generator will create static properties for each operator with [TypeOption] attribute.
/// </summary>
/// <remarks>
/// <para>
/// This collection provides compile-time discovery of all filter operator types.
/// No switch statements needed - operators know their own SQL/OData representations!
/// </para>
/// <para>
/// Example generated properties:
/// <list type="bullet">
/// <item>FilterOperators.Equal - Equal operator (=, eq)</item>
/// <item>FilterOperators.NotEqual - Not equal operator (&lt;&gt;, ne)</item>
/// <item>FilterOperators.Contains - Contains operator (LIKE, contains)</item>
/// <item>FilterOperators.GreaterThan - Greater than operator (&gt;, gt)</item>
/// </list>
/// </para>
/// </remarks>
[TypeCollection(typeof(FilterOperatorBase), typeof(FilterOperatorBase), typeof(FilterOperators))]
public abstract partial class FilterOperators : TypeCollectionBase<FilterOperatorBase, FilterOperatorBase>
{
    // Source generator creates:
    // - Static constructor
    // - Static properties for each [TypeOption] operator
    // - All() method
    // - GetByName() method
    // - GetById() method
}
```

### Generated Code (Example)

The source generator creates properties like:

```csharp
public partial class FilterOperators
{
    public static FilterOperatorBase Equal { get; } = new EqualOperator();
    public static FilterOperatorBase NotEqual { get; } = new NotEqualOperator();
    public static FilterOperatorBase Contains { get; } = new ContainsOperator();
    // ... etc.

    public static IEnumerable<FilterOperatorBase> All() { /* ... */ }
    public static FilterOperatorBase GetByName(string name) { /* ... */ }
    public static FilterOperatorBase GetById(int id) { /* ... */ }
}
```

### Automatic Discovery - No Manual Registration!

**Key Benefit:** Add a new operator, and it's automatically available:

```csharp
// 1. Create operator class with [TypeOption] attribute
[TypeOption(typeof(FilterOperators), "MyCustom")]
public sealed class MyCustomOperator : FilterOperatorBase { /* ... */ }

// 2. Build project - source generator runs

// 3. Use immediately - no registration code needed!
var op = FilterOperators.MyCustom;  // Property auto-generated!
```

## 7. SQL Formatting Patterns

### Pattern 1: Simple Parameter (Default)

**Use Case:** Direct parameter substitution (=, <>, >, <, >=, <=)

```csharp
// Default implementation - no override needed
public virtual string FormatSqlParameter(string paramName)
    => $"@{paramName}";

// SQL Result:
// WHERE Age = @Age
// WHERE Price > @Price
```

### Pattern 2: LIKE Wildcards

**Use Case:** Pattern matching operators (Contains, StartsWith, EndsWith)

```csharp
// Contains - both sides
public override string FormatSqlParameter(string paramName)
    => $"'%' + @{paramName} + '%'";
// SQL: WHERE Name LIKE '%' + @Name + '%'

// StartsWith - trailing wildcard
public override string FormatSqlParameter(string paramName)
    => $"@{paramName} + '%'";
// SQL: WHERE Name LIKE @Name + '%'

// EndsWith - leading wildcard
public override string FormatSqlParameter(string paramName)
    => $"'%' + @{paramName}";
// SQL: WHERE Name LIKE '%' + @Name
```

**Important:** Wildcards are concatenated in SQL, not in parameter value:
- **Correct**: `'%' + @Name + '%'` (SQL concatenates)
- **Incorrect**: Putting `%value%` in the parameter (vulnerable to injection)

### Pattern 3: No Parameter (Null Checks)

**Use Case:** Operators that don't need values (IS NULL, IS NOT NULL)

```csharp
public override string FormatSqlParameter(string paramName)
    => string.Empty;

// SQL Result:
// WHERE Email IS NULL
// WHERE Phone IS NOT NULL
```

### Pattern 4: Multi-Value (IN clause)

**Use Case:** Collection-based filtering

```csharp
public override string FormatSqlParameter(string paramName)
{
    // Translator expands to multiple parameters
    return $"@{paramName}";
}

// SQL Result (translator handles expansion):
// WHERE Status IN (@Status_0, @Status_1, @Status_2)
```

**Note:** The actual parameter expansion is handled by the SQL translator, not the operator.

### Pattern 5: Range (BETWEEN) - Future Example

**Hypothetical BETWEEN operator:**

```csharp
public override string FormatSqlParameter(string paramName)
    => $"@{paramName}_min AND @{paramName}_max";

// SQL Result:
// WHERE Price BETWEEN @Price_min AND @Price_max
```

### SQL Injection Prevention

**Critical Rule:** Never concatenate user values directly into SQL strings.

**Safe Patterns:**
- Use parameterized queries: `@paramName`
- Concatenate wildcards in SQL: `'%' + @param + '%'`
- Let translator handle parameter values

**Unsafe Patterns:**
- `$"'{value}'"` in SQL (opens SQL injection risk)
- Manually escaping quotes (error-prone)

## 8. OData Formatting Patterns

### Pattern 1: String Values

**Rule:** Wrap in single quotes, escape embedded quotes

```csharp
string str => $"'{str.Replace("'", "''")}'",

// Examples:
// "Hello" -> "'Hello'"
// "It's" -> "'It''s'"  (quote escaped as double-quote)
```

### Pattern 2: Numeric Values

**Rule:** No quotes, direct ToString()

```csharp
int or long or short or byte => value.ToString()!,
decimal or double or float => value.ToString()!,

// Examples:
// 123 -> "123"
// 45.67 -> "45.67"
```

### Pattern 3: Boolean Values

**Rule:** Lowercase true/false

```csharp
bool b => b.ToString().ToLowerInvariant(),

// Examples:
// true -> "true"
// false -> "false"
```

### Pattern 4: DateTime Values

**Rule:** Prefix with type, format as ISO 8601

```csharp
DateTime dt => $"datetime'{dt:yyyy-MM-ddTHH:mm:ss}'",
DateTimeOffset dto => $"datetimeoffset'{dto:yyyy-MM-ddTHH:mm:sszzz}'",

// Examples:
// DateTime(2024, 1, 15, 10, 30, 0) -> "datetime'2024-01-15T10:30:00'"
// DateTimeOffset -> "datetimeoffset'2024-01-15T10:30:00-05:00'"
```

### Pattern 5: Guid Values

**Rule:** Prefix with 'guid', wrap in quotes

```csharp
Guid guid => $"guid'{guid}'",

// Example:
// Guid.Parse("...") -> "guid'123e4567-e89b-12d3-a456-426614174000'"
```

### Pattern 6: Null Values

**Rule:** Literal "null" (no quotes)

```csharp
if (value == null)
    return "null";
```

### Pattern 7: Collections (IN operator)

**Rule:** Comma-separated values in parentheses

```csharp
var values = enumerable.Cast<object>()
    .Select(v => v switch {
        string str => $"'{str.Replace("'", "''")}'",
        int or long => v.ToString(),
        _ => $"'{v}'"
    });

return $"({string.Join(",", values)})";

// Example:
// ["Active", "Pending"] -> "('Active','Pending')"
// [1, 2, 3] -> "(1,2,3)"
```

### OData Special Characters

**Characters Requiring Escaping:**
- Single quote `'` -> `''` (double-quote)
- No escaping needed for `&`, `?`, `=`, etc. (handled by OData URL encoding)

## 9. Item Template Parameters

If creating a **Visual Studio Item Template** for FilterOperators, use these parameters:

### Template Parameters

| Parameter                  | Type    | Example        | Description                                |
|----------------------------|---------|----------------|--------------------------------------------|
| `$OperatorName$`           | string  | "Equal"        | Operator name (PascalCase)                 |
| `$OperatorId$`             | int     | 1              | Unique operator ID                         |
| `$SqlOperator$`            | string  | "="            | SQL operator syntax                        |
| `$ODataOperator$`          | string  | "eq"           | OData operator syntax                      |
| `$RequiresValue$`          | bool    | true           | Whether operator needs a value             |
| `$HasCustomParameterFormat$` | bool  | false          | Override FormatSqlParameter()?             |
| `$CustomParameterFormat$`  | string  | "'%' + @{0} + '%'" | SQL parameter format (if custom)       |

### Template Structure

**File:** `FilterOperator.cs`

```csharp
using System;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// $OperatorName$ operator ($SqlOperator$, $ODataOperator$).
/// </summary>
[TypeOption(typeof(FilterOperators), "$OperatorName$")]
public sealed class $OperatorName$Operator : FilterOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="$OperatorName$Operator"/> class.
    /// </summary>
    public $OperatorName$Operator()
        : base(
            id: $OperatorId$,
            name: "$OperatorName$",
            sqlOperator: "$SqlOperator$",
            odataOperator: "$ODataOperator$",
            requiresValue: $RequiresValue$)
    {
    }

#if $HasCustomParameterFormat$
    /// <summary>
    /// Formats SQL parameter with custom pattern.
    /// </summary>
    public override string FormatSqlParameter(string paramName)
        => $"$CustomParameterFormat$";
#endif

    /// <inheritdoc/>
    public override string FormatODataValue(object? value)
    {
        if (value == null)
            return "null";

        return value switch
        {
            string str => $"'{str.Replace("'", "''")}'",
            int or long or short or byte => value.ToString()!,
            decimal or double or float => value.ToString()!,
            bool b => b.ToString().ToLowerInvariant(),
            DateTime dt => $"datetime'{dt:yyyy-MM-ddTHH:mm:ss}'",
            DateTimeOffset dto => $"datetimeoffset'{dto:yyyy-MM-ddTHH:mm:sszzz}'",
            Guid guid => $"guid'{guid}'",
            _ => $"'{value}'"
        };
    }
}
```

## 10. XML Documentation Standards

### Class Documentation

```csharp
/// <summary>
/// [Operator description] operator ([SQL syntax], [OData syntax]).
/// [Optional: Special behavior notes].
/// </summary>
/// <remarks>
/// <para>
/// [Optional: Detailed explanation of operator behavior]
/// </para>
/// <para>
/// Usage example:
/// <code>
/// var condition = new FilterCondition {
///     PropertyName = "FieldName",
///     Operator = FilterOperators.[OperatorName],
///     Value = [example value]
/// };
/// </code>
/// </para>
/// </remarks>
```

**Example:**

```csharp
/// <summary>
/// Equal operator (=, eq).
/// No switch statements needed - operator knows its own representations!
/// </summary>
```

### Constructor Documentation

```csharp
/// <summary>
/// Initializes a new instance of the <see cref="[OperatorName]Operator"/> class.
/// </summary>
```

### Method Documentation

**FormatSqlParameter():**

```csharp
/// <summary>
/// Formats SQL parameter [with specific behavior description].
/// </summary>
```

Examples:
- "Formats SQL parameter with wildcards for LIKE pattern matching."
- "Formats SQL parameter with trailing wildcard."
- "Returns empty string since IS NULL doesn't use parameters."

**FormatODataValue():**

```csharp
/// <inheritdoc/>
```

Or if providing additional context:

```csharp
/// <summary>
/// Formats the value for OData query strings.
/// [Optional: Special handling notes].
/// </summary>
/// <param name="value">The value to format.</param>
/// <returns>The formatted OData value string.</returns>
```

## 11. Common Patterns and Best Practices

### Pattern 1: Type-Safe Switch Expressions

Use C# pattern matching for clean, type-safe value formatting:

```csharp
return value switch
{
    string str => $"'{str.Replace("'", "''")}'",
    int or long or short or byte => value.ToString()!,
    decimal or double or float => value.ToString()!,
    bool b => b.ToString().ToLowerInvariant(),
    DateTime dt => $"datetime'{dt:yyyy-MM-ddTHH:mm:ss}'",
    _ => $"'{value}'"  // Fallback
};
```

### Pattern 2: Null Handling

Always handle null first:

```csharp
public override string FormatODataValue(object? value)
{
    if (value == null)
        return "null";

    // Then handle non-null cases
}
```

### Pattern 3: Quote Escaping

**Always** escape single quotes in string values:

```csharp
string str => $"'{str.Replace("'", "''")}'",
```

This prevents OData syntax errors:
- Input: `"It's great"`
- Output: `'It''s great'` (valid OData)

### Pattern 4: SQL Wildcard Concatenation

Concatenate wildcards in SQL, not in parameter value:

**Correct:**
```csharp
public override string FormatSqlParameter(string paramName)
    => $"'%' + @{paramName} + '%'";
```

**Incorrect:**
```csharp
// DON'T DO THIS - vulnerable to SQL injection
public override string FormatSqlParameter(string paramName)
    => $"'%{value}%'";  // NO! Never embed values
```

### Pattern 5: Sealed Classes

Always mark operator classes as `sealed`:

```csharp
public sealed class EqualOperator : FilterOperatorBase
```

Operators should be final implementations, not inheritance hierarchies.

### Pattern 6: Parameterless Constructors

Operators should have parameterless constructors only:

```csharp
public EqualOperator()
    : base(id: 1, name: "Equal", ...)
{
}
```

This allows TypeCollection to instantiate them via `new()` constraint.

## 12. Common Mistakes and How to Avoid Them

### Mistake 1: Forgetting [TypeOption] Attribute

**Problem:**
```csharp
// Missing attribute!
public sealed class MyOperator : FilterOperatorBase
```

**Solution:**
```csharp
[TypeOption(typeof(FilterOperators), "MyOperator")]
public sealed class MyOperator : FilterOperatorBase
```

**Symptom:** Operator not available on `FilterOperators` class, no compile error.

### Mistake 2: Name Mismatch

**Problem:**
```csharp
[TypeOption(typeof(FilterOperators), "Equal")]  // Says "Equal"
public EqualOperator()
    : base(name: "Equals", ...)  // Says "Equals" - MISMATCH!
```

**Solution:** Ensure exact name match:
```csharp
[TypeOption(typeof(FilterOperators), "Equal")]
public EqualOperator()
    : base(name: "Equal", ...)  // Matches!
```

### Mistake 3: SQL Injection Vulnerability

**Problem:**
```csharp
// Embedding user values directly
public override string FormatSqlParameter(string paramName)
    => $"'{userValue}'";  // DANGEROUS!
```

**Solution:**
```csharp
// Use parameterized queries
public override string FormatSqlParameter(string paramName)
    => $"@{paramName}";
```

### Mistake 4: Not Escaping OData Quotes

**Problem:**
```csharp
string str => $"'{str}'",  // Doesn't escape quotes!
```

**Solution:**
```csharp
string str => $"'{str.Replace("'", "''")}'",  // Escapes quotes
```

**Impact:** OData parsing errors with values like "It's great".

### Mistake 5: Incorrect Operator Precedence

**Problem:** Using wrong SQL operator syntax

```csharp
// Wrong SQL for "not equal"
sqlOperator: "!="  // Not standard SQL
```

**Solution:**
```csharp
// Correct SQL syntax
sqlOperator: "<>"  // Standard SQL not-equal
```

### Mistake 6: Missing XML Documentation

**Problem:**
```csharp
// No documentation
public sealed class MyOperator : FilterOperatorBase
```

**Solution:**
```csharp
/// <summary>
/// My custom operator (MYOP, myop).
/// </summary>
public sealed class MyOperator : FilterOperatorBase
```

**Impact:** Poor IntelliSense experience, harder maintenance.

### Mistake 7: Wrong RequiresValue Setting

**Problem:**
```csharp
// IS NULL should not require value
public IsNullOperator()
    : base(requiresValue: true, ...)  // WRONG!
```

**Solution:**
```csharp
public IsNullOperator()
    : base(requiresValue: false, ...)  // Correct
```

### Mistake 8: Not Using Sealed Modifier

**Problem:**
```csharp
public class EqualOperator : FilterOperatorBase  // Should be sealed
```

**Solution:**
```csharp
public sealed class EqualOperator : FilterOperatorBase
```

**Reason:** Operators are final implementations; inheritance not intended.

## 13. Testing Your New Operator

### Unit Test Structure

```csharp
using Xunit;
using FractalDataWorks.Commands.Data;

public class MyOperatorTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var op = new MyOperator();

        // Assert
        Assert.Equal(expectedId, op.Id);
        Assert.Equal("MyOperator", op.Name);
        Assert.Equal("EXPECTED_SQL", op.SqlOperator);
        Assert.Equal("expected_odata", op.ODataOperator);
        Assert.Equal(expectedRequiresValue, op.RequiresValue);
    }

    [Theory]
    [InlineData("paramName", "expectedFormat")]
    public void FormatSqlParameter_FormatsCorrectly(string paramName, string expected)
    {
        // Arrange
        var op = new MyOperator();

        // Act
        var result = op.FormatSqlParameter(paramName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("text", "'text'")]
    [InlineData("it's", "'it''s'")]  // Test quote escaping
    [InlineData(123, "123")]
    [InlineData(null, "null")]
    public void FormatODataValue_FormatsCorrectly(object value, string expected)
    {
        // Arrange
        var op = new MyOperator();

        // Act
        var result = op.FormatODataValue(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TypeCollection_ContainsOperator()
    {
        // Verify TypeCollection integration
        var op = FilterOperators.MyOperator;

        Assert.NotNull(op);
        Assert.IsType<MyOperator>(op);
    }
}
```

### Integration Test

```csharp
[Fact]
public void FilterCondition_UsesOperator_BuildsCorrectSql()
{
    // Arrange
    var condition = new FilterCondition
    {
        PropertyName = "Name",
        Operator = FilterOperators.MyOperator,
        Value = "test"
    };

    // Act
    var sql = $"[{condition.PropertyName}] {condition.Operator.SqlOperator} {condition.Operator.FormatSqlParameter(condition.PropertyName)}";

    // Assert
    Assert.Equal("expected SQL string", sql);
}
```

## 14. Quick Reference Checklist

When creating a new FilterOperator, verify:

- [ ] File created in `src/FractalDataWorks.Commands.Data/Operators/`
- [ ] File name: `{OperatorName}Operator.cs`
- [ ] Required using statements added
- [ ] Namespace: `FractalDataWorks.Commands.Data`
- [ ] `[TypeOption(typeof(FilterOperators), "OperatorName")]` attribute present
- [ ] Class is `public sealed`
- [ ] Extends `FilterOperatorBase`
- [ ] Constructor calls base with all 5 parameters (id, name, sqlOperator, odataOperator, requiresValue)
- [ ] Name parameter matches [TypeOption] string exactly
- [ ] Unique ID assigned (check existing operators)
- [ ] `FormatSqlParameter()` overridden if needed (wildcards, ranges, null)
- [ ] `FormatODataValue()` implemented (required)
- [ ] OData string values escaped: `Replace("'", "''")`
- [ ] XML documentation on class
- [ ] XML documentation on constructor
- [ ] XML documentation on overridden methods
- [ ] Unit tests created
- [ ] Build succeeds
- [ ] `FilterOperators.OperatorName` property available (TypeCollection integration)

## 15. Additional Resources

### Related Files

- `src/FractalDataWorks.Commands.Data.Abstractions/Operators/FilterOperatorBase.cs` - Base class
- `src/FractalDataWorks.Commands.Data.Abstractions/Operators/FilterOperators.cs` - TypeCollection
- `src/FractalDataWorks.Commands.Data/Operators/*.cs` - All operator implementations

### TypeCollection Documentation

- TypeCollection pattern eliminates reflection
- Source generator creates static properties at compile-time
- `[TypeOption]` attribute registers types for discovery

### SQL and OData References

**SQL Operators:**
- `=`, `<>`, `>`, `<`, `>=`, `<=` - Comparison
- `LIKE` - Pattern matching (with wildcards `%`, `_`)
- `IN` - Set membership
- `BETWEEN` - Range
- `IS NULL`, `IS NOT NULL` - Null checks

**OData Operators:**
- `eq`, `ne`, `gt`, `lt`, `ge`, `le` - Comparison
- `contains`, `startswith`, `endswith` - String functions
- `in` - Set membership

### Example Query Translations

| Operator      | SQL Example                           | OData Example                                |
|---------------|---------------------------------------|----------------------------------------------|
| Equal         | `WHERE Age = @Age`                    | `?$filter=Age eq 25`                         |
| Contains      | `WHERE Name LIKE '%' + @Name + '%'`   | `?$filter=contains(Name, 'Acme')`            |
| GreaterThan   | `WHERE Price > @Price`                | `?$filter=Price gt 100`                      |
| IsNull        | `WHERE Email IS NULL`                 | `?$filter=Email eq null`                     |
| In            | `WHERE Status IN (@S_0, @S_1)`        | `?$filter=Status in ('Active','Pending')`    |

---

## Summary

FilterOperators use the **TypeCollection pattern** to replace enums with behavior-rich classes that:

1. **Eliminate switch statements** - operators know their own SQL/OData representations
2. **Enable extensibility** - add new operators without modifying existing code
3. **Provide type safety** - compile-time checking via source-generated properties
4. **Handle formatting** - each operator formats its own SQL parameters and OData values

**Creating a new operator is straightforward:**
1. Extend `FilterOperatorBase`
2. Add `[TypeOption]` attribute
3. Implement constructor and `FormatODataValue()`
4. Override `FormatSqlParameter()` if needed
5. Build - TypeCollection integration is automatic!

The result is cleaner, more maintainable code that's easy to extend and impossible to misuse.
