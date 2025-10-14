using System;
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Greater than or equal operator (&gt;=, ge).
/// </summary>
[TypeOption(typeof(FilterOperators), "GreaterThanOrEqual")]
public sealed class GreaterThanOrEqualOperator : FilterOperatorBase
{
    public GreaterThanOrEqualOperator()
        : base(
            id: 7,
            name: "GreaterThanOrEqual",
            sqlOperator: ">=",
            odataOperator: "ge",
            requiresValue: true)
    {
    }

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
