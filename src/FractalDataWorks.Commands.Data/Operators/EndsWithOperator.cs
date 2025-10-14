using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// EndsWith operator (LIKE '%value', endswith).
/// </summary>
[TypeOption(typeof(FilterOperators), "EndsWith")]
public sealed class EndsWithOperator : FilterOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EndsWithOperator"/> class.
    /// </summary>
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
    public override string FormatSqlParameter(string paramName) => $"'%' + @{paramName}";

    /// <inheritdoc/>
    public override string FormatODataValue(object? value)
    {
        return $"'{value?.ToString()?.Replace("'", "''")}'";
    }
}
