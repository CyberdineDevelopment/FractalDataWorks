using System;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Samples.MockTypes;

/// <summary>
/// Demonstrates the DataTypeConverterType TypeCollection pattern for SQL to CLR type mapping.
/// </summary>
/// <remarks>
/// <para><strong>What This Demonstrates:</strong></para>
/// <list type="bullet">
///   <item><description>
///     <strong>Type Mapping Metadata:</strong> Converters define bidirectional mappings between data store
///     types (SQL "int", JSON "integer", XML "xs:int") and CLR types (System.Int32).
///   </description></item>
///   <item><description>
///     <strong>Configuration-Driven Type Resolution:</strong> Schema configuration specifies ConverterTypeName
///     as "SqlInt32" (not the implementing class), enabling runtime type mapping without reflection.
///   </description></item>
///   <item><description>
///     <strong>Cross-Assembly Discovery:</strong> DataTypeConverterTypes uses RestrictToCurrentCompilation=false,
///     allowing converters from multiple assemblies to be discovered and used together.
///   </description></item>
///   <item><description>
///     <strong>Null Safety Integration:</strong> Converters work with the field schema's IsNullable flag to
///     handle nullable value types (int vs int?) correctly.
///   </description></item>
/// </list>
/// <para><strong>In Production Use:</strong></para>
/// <para>
/// Schema configuration references this as:
/// <code>
/// "Fields": [
///   {
///     "Name": "OrderId",
///     "ConverterTypeName": "SqlInt32",  // Resolved to this type at runtime
///     "Role": "Identity"
///   }
/// ]
/// </code>
/// </para>
/// </remarks>
[TypeOption(typeof(DataTypeConverterTypes), "SqlInt32")]
public sealed class MockSqlInt32ConverterType : DataTypeConverterTypeBase
{
    public MockSqlInt32ConverterType()
        : base(
            id: 1,
            name: "SqlInt32",
            displayName: "SQL Int32 Converter",
            description: "Converts SQL Server int type to CLR System.Int32",
            sourceTypeName: "int",
            targetClrType: typeof(int))
    {
    }
}
