using System;
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Less than operator (&lt;, lt).
/// </summary>
[TypeOption(typeof(FilterOperators), "LessThan")]
public sealed class LessThanOperator : FilterOperatorBase
{
    public LessThanOperator()
        : base(
            id: 8,
            name: "LessThan",
            sqlOperator: "<",
            odataOperator: "lt",
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
