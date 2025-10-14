# Hierarchical Filter Expression Design

## Problem: Current FilterExpression is Flat

The current implementation only supports flat filter conditions with a single logical operator:

```csharp
public interface IFilterExpression
{
    IReadOnlyList<FilterCondition> Conditions { get; }
    LogicalOperator? LogicalOperator { get; }
}
```

This can't represent complex hierarchical logic like:
```sql
WHERE (Age > 18 AND Country = 'US') OR (Age > 21 AND Country = 'Canada')
```

## Solution: Composite Pattern - Filter Expression Tree

### New Interface Design

```csharp
/// <summary>
/// Base interface for filter expressions (leaf or composite).
/// Enables hierarchical filter trees using Composite Pattern.
/// </summary>
public interface IFilterExpression
{
    /// <summary>
    /// Gets the type of this filter expression.
    /// </summary>
    FilterExpressionType ExpressionType { get; }
}

/// <summary>
/// Types of filter expressions in the tree.
/// </summary>
public enum FilterExpressionType
{
    Condition,      // Leaf node: PropertyName Op Value
    Logical,        // Branch node: AND/OR with children
    Not             // Unary node: NOT with single child
}

/// <summary>
/// Leaf node: A single filter condition (PropertyName Op Value).
/// </summary>
public interface IFilterCondition : IFilterExpression
{
    string PropertyName { get; }
    FilterOperatorBase Operator { get; }
    object? Value { get; }
}

/// <summary>
/// Branch node: Combines multiple filter expressions with AND/OR.
/// </summary>
public interface ILogicalFilterExpression : IFilterExpression
{
    LogicalOperator Operator { get; }
    IReadOnlyList<IFilterExpression> Children { get; }
}

/// <summary>
/// Unary node: Negates a filter expression.
/// </summary>
public interface INotFilterExpression : IFilterExpression
{
    IFilterExpression Child { get; }
}
```

### Example: Complex Hierarchical Filter

```csharp
// Represents: (Age > 18 AND Country = 'US') OR (Age > 21 AND Country = 'Canada')

var filter = new LogicalFilterExpression
{
    Operator = LogicalOperator.Or,
    Children = new IFilterExpression[]
    {
        // First branch: Age > 18 AND Country = 'US'
        new LogicalFilterExpression
        {
            Operator = LogicalOperator.And,
            Children = new IFilterExpression[]
            {
                new FilterCondition
                {
                    PropertyName = "Age",
                    Operator = FilterOperators.GreaterThan,
                    Value = 18
                },
                new FilterCondition
                {
                    PropertyName = "Country",
                    Operator = FilterOperators.Equal,
                    Value = "US"
                }
            }
        },
        // Second branch: Age > 21 AND Country = 'Canada'
        new LogicalFilterExpression
        {
            Operator = LogicalOperator.And,
            Children = new IFilterExpression[]
            {
                new FilterCondition
                {
                    PropertyName = "Age",
                    Operator = FilterOperators.GreaterThan,
                    Value = 21
                },
                new FilterCondition
                {
                    PropertyName = "Country",
                    Operator = FilterOperators.Equal,
                    Value = "Canada"
                }
            }
        }
    }
};
```

### Tree Visualization

```
              OR (root)
             /  \
            /    \
           /      \
        AND       AND
       /  \       /  \
      /    \     /    \
  Age>18  US  Age>21  Canada
  (leaf)  (leaf) (leaf) (leaf)
```

### Translation to SQL

```csharp
public class SqlFilterTranslator
{
    public string Translate(IFilterExpression expression)
    {
        return expression.ExpressionType switch
        {
            FilterExpressionType.Condition => TranslateCondition((IFilterCondition)expression),
            FilterExpressionType.Logical => TranslateLogical((ILogicalFilterExpression)expression),
            FilterExpressionType.Not => TranslateNot((INotFilterExpression)expression),
            _ => throw new NotSupportedException()
        };
    }

    private string TranslateCondition(IFilterCondition condition)
    {
        return $"{condition.PropertyName} {condition.Operator.SqlOperator} {condition.Operator.FormatSqlParameter(condition.PropertyName)}";
    }

    private string TranslateLogical(ILogicalFilterExpression logical)
    {
        var children = logical.Children.Select(Translate);
        var combined = string.Join($" {logical.Operator.SqlOperator} ", children);
        return $"({combined})";  // Wrap in parentheses for proper precedence
    }

    private string TranslateNot(INotFilterExpression not)
    {
        return $"NOT ({Translate(not.Child)})";
    }
}
```

**Result:**
```sql
((Age > @Age AND Country = @Country) OR (Age > @Age2 AND Country = @Country2))
```

## Benefits of Tree Structure

### 1. **Arbitrary Complexity**
Support any level of nesting:
```
(A AND B) OR ((C OR D) AND NOT E)
```

### 2. **Proper Operator Precedence**
Each node knows its scope, preventing ambiguity.

### 3. **Visitor Pattern for Translation**
Translators can walk the tree recursively:
```csharp
public interface IFilterExpressionVisitor<T>
{
    T Visit(IFilterCondition condition);
    T Visit(ILogicalFilterExpression logical);
    T Visit(INotFilterExpression not);
}
```

### 4. **Query Optimization**
Tree structure enables optimization:
- Flatten unnecessary nesting
- Push conditions down to data source
- Eliminate redundant conditions
- Reorder for index usage

### 5. **Better JSON Representation**
```json
{
  "type": "Logical",
  "operator": "Or",
  "children": [
    {
      "type": "Logical",
      "operator": "And",
      "children": [
        {
          "type": "Condition",
          "property": "Age",
          "operator": "GreaterThan",
          "value": 18
        },
        {
          "type": "Condition",
          "property": "Country",
          "operator": "Equal",
          "value": "US"
        }
      ]
    }
  ]
}
```

## Hierarchical Data & Joins

For hierarchical data, we need similar tree structures:

### Nested Projections
```csharp
public interface IProjectionExpression
{
    IReadOnlyList<IProjectionField> Fields { get; }
}

public interface IProjectionField
{
    string PropertyName { get; }
    IProjectionExpression? NestedProjection { get; }  // For related data!
}
```

**Example:**
```csharp
SELECT
  Id,
  Name,
  Orders.{        // Nested projection
    OrderId,
    Total,
    Items.{       // Deeply nested
      ProductName,
      Quantity
    }
  }
```

### Hierarchical Joins
```csharp
public interface IJoinExpression
{
    string TargetContainer { get; }
    string JoinType { get; }  // "Inner", "Left", "Right"
    IFilterExpression OnCondition { get; }
    IReadOnlyList<IJoinExpression> NestedJoins { get; }  // Recursive!
}
```

**Example:**
```csharp
Customer
  JOIN Orders ON Orders.CustomerId = Customer.Id
    JOIN OrderItems ON OrderItems.OrderId = Orders.Id
      JOIN Products ON Products.Id = OrderItems.ProductId
```

## Aggregation Trees

For complex aggregations:

```csharp
SELECT
  Country,
  SUM(Revenue) as TotalRevenue,
  AVG(OrderValue) as AvgOrder,
  Customers.{         // Nested aggregation
    Type,
    COUNT(*) as Count,
    SUM(Lifetime Value) as TotalValue
  }
GROUP BY Country, Customers.Type
```

## Migration Strategy

### Phase 1: Backward Compatibility (Current)
Keep current flat FilterExpression for simple cases.

### Phase 2: Add Tree Support
Introduce IFilterExpressionNode with Composite Pattern.

### Phase 3: Fluent Builder API
```csharp
var filter = FilterBuilder
    .Create()
    .Or(
        builder => builder
            .And()
            .Condition("Age", Op.GreaterThan, 18)
            .Condition("Country", Op.Equal, "US"),
        builder => builder
            .And()
            .Condition("Age", Op.GreaterThan, 21)
            .Condition("Country", Op.Equal, "Canada")
    )
    .Build();
```

### Phase 4: LINQ Provider (Future)
```csharp
var filter = customers
    .Where(c => (c.Age > 18 && c.Country == "US") ||
                (c.Age > 21 && c.Country == "Canada"))
    .ToDataCommandFilter();
```

## Conclusion

**YES - DataCommand should be a tree structure!**

Current implementation is **Phase 1** - supporting simple, flat queries.

The architecture is **designed for Phase 2** - the interfaces can be extended to support hierarchical filters, joins, and projections without breaking existing code.

This is the power of the abstraction - we can start simple and evolve to handle complex, real-world scenarios as needed.
