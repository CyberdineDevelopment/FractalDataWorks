using System;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Converts SQL bit to CLR Boolean.
/// </summary>
[TypeOption(typeof(DataTypeConverters), "SqlBoolean")]
public sealed class SqlBooleanConverter : DataTypeConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlBooleanConverter"/> class.
    /// </summary>
    public SqlBooleanConverter()
        : base(5, "SqlBoolean")
    {
    }

    /// <summary>
    /// Gets the source type name.
    /// </summary>
    public override string SourceTypeName => "bit";

    /// <summary>
    /// Gets the target CLR type.
    /// </summary>
    public override Type TargetClrType => typeof(bool);

    /// <summary>
    /// Converts from SQL bit to CLR Boolean.
    /// </summary>
    public override object? Convert(object? value)
    {
        if (value is null or DBNull)
        {
            return null;
        }

        return System.Convert.ToBoolean(value);
    }

    /// <summary>
    /// Converts from CLR Boolean back to SQL bit.
    /// </summary>
    public override object? ConvertBack(object? clrValue)
    {
        return clrValue;
    }
}
