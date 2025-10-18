using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Samples.MockTypes;

/// <summary>
/// Demonstrates the DataTransformerType TypeCollection pattern for ETL transformations.
/// </summary>
/// <remarks>
/// <para><strong>What This Demonstrates:</strong></para>
/// <list type="bullet">
///   <item><description>
///     <strong>ETL Pipeline Integration:</strong> Transformers sit between data extraction and command execution,
///     applying business logic (aggregation, filtering, calculations, enrichment) to extracted data.
///   </description></item>
///   <item><description>
///     <strong>Field Role Awareness:</strong> Aggregation transformers operate specifically on "Measure" fields
///     (FieldRole.Measure), demonstrating how field roles enable type-safe, metadata-driven transformations.
///   </description></item>
///   <item><description>
///     <strong>Streaming vs. Buffering:</strong> SupportsStreaming=false indicates this transformer must buffer
///     all data to calculate aggregates (vs. transformers like "Filter" which can stream row-by-row).
///   </description></item>
///   <item><description>
///     <strong>Composable Pipelines:</strong> Multiple transformers can chain together via IDataTransformer&lt;TResult, TInput&gt;
///     to build complex data processing pipelines declaratively.
///   </description></item>
/// </list>
/// <para><strong>In Production Use:</strong></para>
/// <para>
/// Configuration would reference this as:
/// <code>
/// "DataPipeline": {
///   "Transformers": [
///     {
///       "TransformerType": "Aggregate",  // Resolved to this type at runtime
///       "Operations": [
///         { "Field": "TotalSales", "Function": "SUM" },
///         { "Field": "OrderCount", "Function": "COUNT" }
///       ]
///     }
///   ]
/// }
/// </code>
/// </para>
/// </remarks>
[TypeOption(typeof(DataTransformerTypes), "Aggregate")]
public sealed class MockAggregateTransformerType : DataTransformerTypeBase
{
    public MockAggregateTransformerType()
        : base(
            id: 1,
            name: "Aggregate",
            displayName: "Aggregate Transformer",
            description: "Performs aggregation operations (SUM, AVG, COUNT, MIN, MAX) on measure fields",
            supportsStreaming: false)
    {
    }
}
