using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Represents a data transformer type definition - metadata about ETL transformers.
/// </summary>
/// <remarks>
/// Transformer types apply ETL-style transformations (aggregation, filtering, calculations, etc.).
/// </remarks>
public interface IDataTransformerType : ITypeOption
{
    /// <summary>
    /// Gets whether this transformer supports streaming.
    /// </summary>
    bool SupportsStreaming { get; }
}
