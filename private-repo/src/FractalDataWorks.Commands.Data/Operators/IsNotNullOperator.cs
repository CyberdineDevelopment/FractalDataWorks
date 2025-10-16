using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// IS NOT NULL operator (IS NOT NULL, ne null).
/// This operator does NOT require a value parameter.
/// </summary>
[TypeOption(typeof(FilterOperators), "IsNotNull")]
public sealed class IsNotNullOperator : FilterOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IsNotNullOperator"/> class.
    /// </summary>
    public IsNotNullOperator()
        : base(
            id: 11,
            name: "IsNotNull",
            sqlOperator: "IS NOT NULL",
            odataOperator: "ne null",
            requiresValue: false)  // No value needed!
    {
    }

    /// <summary>
    /// Returns empty string since IS NOT NULL doesn't use parameters.
    /// </summary>
    public override string FormatSqlParameter(string paramName) => string.Empty;

    /// <summary>
    /// Returns empty string since OData handles "ne null" without formatting.
    /// </summary>
    public override string FormatODataValue(object? value) => string.Empty;
}
