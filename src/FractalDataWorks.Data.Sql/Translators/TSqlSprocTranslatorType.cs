using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Translator type for T-SQL stored procedure execution.
/// </summary>
[TypeOption(typeof(TranslatorTypes), "TSqlSproc")]
public sealed class TSqlSprocTranslatorType : TranslatorTypeBase
{
    /// <summary>
    /// Singleton instance of TSqlSprocTranslatorType.
    /// </summary>
    public static readonly TSqlSprocTranslatorType Instance = new();

    private TSqlSprocTranslatorType()
        : base(
            id: 2,
            name: "TSqlSproc",
            displayName: "T-SQL Stored Procedure Translator",
            description: "Translates universal data requests to T-SQL EXEC statements for stored procedures",
            domain: "Sql")
    {
    }
}
