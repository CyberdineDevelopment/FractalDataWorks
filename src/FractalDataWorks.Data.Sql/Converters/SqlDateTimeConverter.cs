using System;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Converts SQL datetime/datetime2 to CLR DateTime.
/// </summary>
[TypeOption(typeof(DataTypeConverters), "SqlDateTime")]
public sealed class SqlDateTimeConverter : DataTypeConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlDateTimeConverter"/> class.
    /// </summary>
    public SqlDateTimeConverter()
        : base(3, "SqlDateTime")
    {
    }

    /// <summary>
    /// Gets the source type name.
    /// </summary>
    public override string SourceTypeName => "datetime";

    /// <summary>
    /// Gets the target CLR type.
    /// </summary>
    public override Type TargetClrType => typeof(DateTime);

    /// <summary>
    /// Converts from SQL datetime to CLR DateTime.
    /// </summary>
    public override object? Convert(object? value)
    {
        if (value is null or DBNull)
        {
            return null;
        }

        return System.Convert.ToDateTime(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts from CLR DateTime back to SQL datetime.
    /// </summary>
    public override object? ConvertBack(object? clrValue)
    {
        return clrValue;
    }
}
