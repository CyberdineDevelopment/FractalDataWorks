using System;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Samples.MsSql;

/// <summary>
/// Production-ready SQL Server int to CLR Int32 converter.
/// </summary>
[TypeOption(typeof(DataTypeConverterTypes), "SqlInt32")]
public sealed class SqlInt32Converter : DataTypeConverterBase
{
    public SqlInt32Converter()
        : base(id: 1, name: "SqlInt32")
    {
    }

    public override string SourceTypeName => "int";
    public override Type TargetClrType => typeof(int);

    public override object? Convert(object? value)
    {
        if (value is null or DBNull)
        {
            return null;
        }

        return System.Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    public override object? ConvertBack(object? clrValue)
    {
        return clrValue;
    }
}
