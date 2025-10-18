using System;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Samples.MockTypes;

/// <summary>
/// Demonstrates the DataTypeConverter pattern for SQL to CLR type mapping.
/// </summary>
/// <remarks>
/// <para><strong>What This Demonstrates:</strong></para>
/// <list type="bullet">
///   <item><description>
///     <strong>Single-Class Pattern:</strong> One class contains both metadata (SourceTypeName, TargetClrType)
///     and implementation (Convert, ConvertBack). No separate "Type" class needed.
///   </description></item>
///   <item><description>
///     <strong>TypeCollection Registration:</strong> The [TypeOption] attribute registers this converter
///     in DataTypeConverterTypes, making it discoverable by name ("SqlInt32").
///   </description></item>
///   <item><description>
///     <strong>Type Mapping:</strong> Bidirectional mapping between SQL "int" and CLR System.Int32.
///   </description></item>
///   <item><description>
///     <strong>Null Handling:</strong> Properly handles DBNull from SQL and null CLR values.
///   </description></item>
///   <item><description>
///     <strong>Configuration-Driven:</strong> Schema configuration references this as "SqlInt32",
///     which is resolved at runtime via DataTypeConverterTypes.GetByName("SqlInt32").
///   </description></item>
///   <item><description>
///     <strong>Cross-Assembly Discovery:</strong> Because DataTypeConverterTypes uses
///     RestrictToCurrentCompilation=false, this converter can be discovered even when
///     defined in a different assembly than the abstractions.
///   </description></item>
/// </list>
/// <para><strong>In Production Use:</strong></para>
/// <para>
/// Schema configuration references this converter:
/// <code>
/// "Fields": [
///   {
///     "Name": "OrderId",
///     "DataType": "int",
///     "ConverterTypeName": "SqlInt32",  // Resolved to this converter at runtime
///     "Role": "Identity"
///   }
/// ]
/// </code>
/// </para>
/// <para><strong>Runtime Usage:</strong></para>
/// <code>
/// var converter = DataTypeConverterTypes.GetByName("SqlInt32");
/// var clrValue = converter.Convert(dbValue);  // Direct call - no switch statement!
/// </code>
/// </remarks>
[TypeOption(typeof(DataTypeConverters), "SqlInt32")]
public sealed class MockSqlInt32Converter : DataTypeConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MockSqlInt32Converter"/> class.
    /// </summary>
    public MockSqlInt32Converter()
        : base(id: 1, name: "SqlInt32")
    {
    }

    /// <summary>
    /// Gets the source type name in SQL Server.
    /// </summary>
    public override string SourceTypeName => "int";

    /// <summary>
    /// Gets the target CLR type.
    /// </summary>
    public override Type TargetClrType => typeof(int);

    /// <summary>
    /// Converts from SQL int to CLR Int32.
    /// </summary>
    /// <param name="value">The SQL value (may be DBNull).</param>
    /// <returns>CLR Int32 value, or null if input was DBNull.</returns>
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
    /// <param name="clrValue">The CLR value.</param>
    /// <returns>Value suitable for SQL parameter.</returns>
    public override object? ConvertBack(object? clrValue)
    {
        // For SQL parameters, just return the value as-is
        // ADO.NET handles the type conversion
        return clrValue;
    }
}
