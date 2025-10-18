using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Samples.MsSql;

/// <summary>
/// Sample aggregation transformer type demonstrating the TypeCollection pattern.
/// </summary>
/// <remarks>
/// In production, this would include actual ETL aggregation logic (SUM, AVG, COUNT, MIN, MAX)
/// operating on measure fields identified by FieldRole.Measure.
/// </remarks>
[TypeOption(typeof(DataTransformerTypes), "Aggregate")]
public sealed class AggregateTransformerType : DataTransformerTypeBase
{
    public AggregateTransformerType()
        : base(
            id: 1,
            name: "Aggregate",
            displayName: "Aggregate Transformer",
            description: "Performs aggregation operations (SUM, AVG, COUNT, MIN, MAX) on measure fields",
            supportsStreaming: false,
            category: "Analytics")
    {
    }
}
