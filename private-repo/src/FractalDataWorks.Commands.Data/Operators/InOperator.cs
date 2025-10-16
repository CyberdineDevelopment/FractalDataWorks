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
        // Translator will expand this to (param1, @param2, @param3, ...)
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
