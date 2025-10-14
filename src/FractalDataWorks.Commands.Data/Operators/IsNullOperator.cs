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
    public override string FormatSqlParameter(string paramName) => string.Empty;

    /// <summary>
    /// Returns empty string since OData handles "eq null" without formatting.
    /// </summary>
    public override string FormatODataValue(object? value) => string.Empty;
}
