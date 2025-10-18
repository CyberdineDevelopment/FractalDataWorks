using System;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Converter type for SQL bit → CLR Boolean.
/// </summary>
[TypeOption(typeof(DataTypeConverterTypes), "SqlBoolean")]
public sealed class SqlBooleanConverterType : DataTypeConverterTypeBase
{
    /// <summary>
    /// Singleton instance of SqlBooleanConverterType.
    /// </summary>
    public static readonly SqlBooleanConverterType Instance = new();

    private SqlBooleanConverterType()
        : base(
            id: 5,
            name: "SqlBoolean",
            displayName: "SQL Boolean Converter",
            description: "Converts SQL bit to CLR Boolean",
            sourceTypeName: "bit",
            targetClrType: typeof(bool))
    {
    }
}
