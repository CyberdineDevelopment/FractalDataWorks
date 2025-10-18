using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Samples.MockTypes;

/// <summary>
/// Demonstrates the TranslatorType TypeCollection pattern for T-SQL query translation.
/// </summary>
/// <remarks>
/// <para><strong>What This Demonstrates:</strong></para>
/// <list type="bullet">
///   <item><description>
///     <strong>Universal to Domain-Specific Translation:</strong> Translators convert universal IDataRequest
///     objects (with filters, sorting, aggregations) into domain-specific query languages (T-SQL, OData, GraphQL).
///   </description></item>
///   <item><description>
///     <strong>No Heuristics Architecture:</strong> Instead of analyzing strings to guess the query type,
///     configuration explicitly specifies "TSqlQuery" as the translator type.
///   </description></item>
///   <item><description>
///     <strong>Domain-Aligned Design:</strong> The "Sql" domain matches the PathType and ContainerType domains,
///     enabling coherent data pipelines (SQL path → SQL container → SQL translator).
///   </description></item>
///   <item><description>
///     <strong>Metadata vs. Implementation Separation:</strong> This type describes the translator's purpose;
///     the actual translation logic lives in a TranslatorBase-derived implementation.
///   </description></item>
/// </list>
/// <para><strong>In Production Use:</strong></para>
/// <para>
/// Configuration would reference this as:
/// <code>
/// "QueryPipeline": {
///   "TranslatorType": "TSqlQuery",  // Resolved to this type at runtime
///   "Filters": [...],
///   "Sorting": [...]
/// }
/// </code>
/// </para>
/// </remarks>
[TypeOption(typeof(TranslatorTypes), "TSqlQuery")]
public sealed class MockTSqlTranslatorType : TranslatorTypeBase
{
    public MockTSqlTranslatorType()
        : base(
            id: 1,
            name: "TSqlQuery",
            displayName: "T-SQL Query Translator",
            description: "Translates universal data requests to T-SQL SELECT queries",
            domain: "Sql")
    {
    }
}
