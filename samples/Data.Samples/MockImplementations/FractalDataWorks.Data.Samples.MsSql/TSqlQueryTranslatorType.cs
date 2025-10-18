using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Samples.MsSql;

/// <summary>
/// Sample T-SQL query translator type demonstrating the TypeCollection pattern.
/// </summary>
/// <remarks>
/// In production, this would include actual universal IDataRequest to T-SQL translation logic,
/// handling filters, sorting, joins, and aggregations.
/// </remarks>
[TypeOption(typeof(TranslatorTypes), "TSqlQuery")]
public sealed class TSqlQueryTranslatorType : TranslatorTypeBase
{
    public TSqlQueryTranslatorType()
        : base(
            id: 1,
            name: "TSqlQuery",
            displayName: "T-SQL Query Translator",
            description: "Translates universal data requests to T-SQL SELECT queries",
            domain: "Sql",
            category: "Query")
    {
    }
}
