using System;
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Less than or equal operator (&lt;=, le).
/// </summary>
[TypeOption(typeof(FilterOperators), "LessThanOrEqual")]
public sealed class LessThanOrEqualOperator : FilterOperatorBase
{
    public LessThanOrEqualOperator()
        : base(
            id: 9,
            name: "LessThanOrEqual",
            sqlOperator: "<=",
            odataOperator: "le",
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
