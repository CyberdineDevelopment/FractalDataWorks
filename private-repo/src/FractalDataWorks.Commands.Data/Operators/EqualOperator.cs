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
