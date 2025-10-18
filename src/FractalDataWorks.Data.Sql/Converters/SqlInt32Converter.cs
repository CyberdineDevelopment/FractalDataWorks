using System;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Converts SQL int to CLR Int32.
/// </summary>
[TypeOption(typeof(DataTypeConverters), "SqlInt32")]
public sealed class SqlInt32Converter : DataTypeConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlInt32Converter"/> class.
    /// </summary>
    public SqlInt32Converter()
        : base(1, "SqlInt32")
    {
    }

    /// <summary>
    /// Gets the source type name.
    /// </summary>
    public override string SourceTypeName => "int";

    /// <summary>
    /// Gets the target CLR type.
    /// </summary>
    public override Type TargetClrType => typeof(int);

    /// <summary>
    /// Converts from SQL int to CLR Int32.
    /// </summary>
    public override object? Convert(object? value)
    {
        if (value is null or DBNull)
        {
            return null;
        }

        return System.Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts from CLR Int32 back to SQL int.
    /// </summary>
    public override object? ConvertBack(object? clrValue)
    {
        return clrValue;
    }
}
