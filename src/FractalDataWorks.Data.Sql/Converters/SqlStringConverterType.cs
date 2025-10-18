using System;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Converter type for SQL nvarchar/varchar → CLR String.
/// </summary>
[TypeOption(typeof(DataTypeConverterTypes), "SqlString")]
public sealed class SqlStringConverterType : DataTypeConverterTypeBase
{
    /// <summary>
    /// Singleton instance of SqlStringConverterType.
    /// </summary>
    public static readonly SqlStringConverterType Instance = new();

    private SqlStringConverterType()
        : base(
            id: 2,
            name: "SqlString",
            displayName: "SQL String Converter",
            description: "Converts SQL nvarchar/varchar to CLR String",
            sourceTypeName: "nvarchar",
            targetClrType: typeof(string))
    {
    }
}
