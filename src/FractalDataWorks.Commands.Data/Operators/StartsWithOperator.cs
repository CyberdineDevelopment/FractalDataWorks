using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

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
    public override string FormatSqlParameter(string paramName) => $"@{paramName} + '%'";

    public override string FormatODataValue(object? value)
    {
        return $"'{value?.ToString()?.Replace("'", "''")}'";
    }
}
