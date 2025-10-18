using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Translator type for T-SQL SELECT queries.
/// </summary>
[TypeOption(typeof(TranslatorTypes), "TSqlQuery")]
public sealed class TSqlQueryTranslatorType : TranslatorTypeBase
{
    /// <summary>
    /// Singleton instance of TSqlQueryTranslatorType.
    /// </summary>
    public static readonly TSqlQueryTranslatorType Instance = new();

    private TSqlQueryTranslatorType()
        : base(
            id: 1,
            name: "TSqlQuery",
            displayName: "T-SQL Query Translator",
            description: "Translates universal data requests to T-SQL SELECT statements",
            domain: "Sql")
    {
    }
}
