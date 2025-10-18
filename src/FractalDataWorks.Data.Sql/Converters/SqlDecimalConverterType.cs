using System;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Converter type for SQL decimal/numeric → CLR Decimal.
/// </summary>
[TypeOption(typeof(DataTypeConverterTypes), "SqlDecimal")]
public sealed class SqlDecimalConverterType : DataTypeConverterTypeBase
{
    /// <summary>
    /// Singleton instance of SqlDecimalConverterType.
    /// </summary>
    public static readonly SqlDecimalConverterType Instance = new();

    private SqlDecimalConverterType()
        : base(
            id: 4,
            name: "SqlDecimal",
            displayName: "SQL Decimal Converter",
            description: "Converts SQL decimal/numeric to CLR Decimal",
            sourceTypeName: "decimal",
            targetClrType: typeof(decimal))
    {
    }
}
