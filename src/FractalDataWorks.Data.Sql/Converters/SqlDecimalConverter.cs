using System;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Converts SQL decimal/numeric to CLR Decimal.
/// </summary>
[TypeOption(typeof(DataTypeConverterTypes), "SqlDecimal")]
public sealed class SqlDecimalConverter : DataTypeConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlDecimalConverter"/> class.
    /// </summary>
    public SqlDecimalConverter()
        : base(4, "SqlDecimal")
    {
    }

    /// <summary>
    /// Gets the source type name.
    /// </summary>
    public override string SourceTypeName => "decimal";

    /// <summary>
    /// Gets the target CLR type.
    /// </summary>
    public override Type TargetClrType => typeof(decimal);

    /// <summary>
    /// Converts from SQL decimal to CLR Decimal.
    /// </summary>
    public override object? Convert(object? value)
    {
        if (value is null or DBNull)
        {
            return null;
        }

        return System.Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts from CLR Decimal back to SQL decimal.
    /// </summary>
    public override object? ConvertBack(object? clrValue)
    {
        return clrValue;
    }
}
