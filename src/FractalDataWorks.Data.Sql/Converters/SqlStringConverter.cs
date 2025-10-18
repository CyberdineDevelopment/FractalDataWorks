using System;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Converts SQL nvarchar/varchar to CLR String.
/// </summary>
[TypeOption(typeof(DataTypeConverters), "SqlString")]
public sealed class SqlStringConverter : DataTypeConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlStringConverter"/> class.
    /// </summary>
    public SqlStringConverter()
        : base(2, "SqlString")
    {
    }

    /// <summary>
    /// Gets the source type name.
    /// </summary>
    public override string SourceTypeName => "nvarchar";

    /// <summary>
    /// Gets the target CLR type.
    /// </summary>
    public override Type TargetClrType => typeof(string);

    /// <summary>
    /// Converts from SQL string to CLR String.
    /// </summary>
    public override object? Convert(object? value)
    {
        if (value is null or DBNull)
        {
            return null;
        }

        return value.ToString();
    }

    /// <summary>
    /// Converts from CLR String back to SQL string.
    /// </summary>
    public override object? ConvertBack(object? clrValue)
    {
        return clrValue;
    }
}
