using System;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Converter type for SQL int → CLR Int32.
/// </summary>
[TypeOption(typeof(DataTypeConverterTypes), "SqlInt32")]
public sealed class SqlInt32ConverterType : DataTypeConverterTypeBase
{
    /// <summary>
    /// Singleton instance of SqlInt32ConverterType.
    /// </summary>
    public static readonly SqlInt32ConverterType Instance = new();

    private SqlInt32ConverterType()
        : base(
            id: 1,
            name: "SqlInt32",
            displayName: "SQL Int32 Converter",
            description: "Converts SQL int to CLR Int32",
            sourceTypeName: "int",
            targetClrType: typeof(int))
    {
    }
}
