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
    public override string FormatSqlParameter(string paramName) => $"'%' + @{paramName} + '%'";

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
