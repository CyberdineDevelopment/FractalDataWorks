using System;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Converter type for SQL datetime/datetime2 → CLR DateTime.
/// </summary>
[TypeOption(typeof(DataTypeConverterTypes), "SqlDateTime")]
public sealed class SqlDateTimeConverterType : DataTypeConverterTypeBase
{
    /// <summary>
    /// Singleton instance of SqlDateTimeConverterType.
    /// </summary>
    public static readonly SqlDateTimeConverterType Instance = new();

    private SqlDateTimeConverterType()
        : base(
            id: 3,
            name: "SqlDateTime",
            displayName: "SQL DateTime Converter",
            description: "Converts SQL datetime/datetime2 to CLR DateTime",
            sourceTypeName: "datetime",
            targetClrType: typeof(DateTime))
    {
    }
}
