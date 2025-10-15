using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Transformers.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Data.Transformers;

/// <summary>
/// DataTransformer transformer for [DESCRIBE TRANSFORMATION].
/// Transforms TInput to TOutput.
/// </summary>
#if (IsGenericTransformer)
[TypeOption(typeof(DataTransformers), "DataTransformer")]
public sealed class DataTransformerTransformer<TIn, TOut> : TransformerBase<TIn, TOut>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataTransformerTransformer{TIn, TOut}"/> class.
    /// </summary>
    public DataTransformerTransformer()
        : base(id: 99, name: "DataTransformer")
    {
    }

    /// <inheritdoc/>
    public override bool SupportsStreaming => SupportsStreaming;

    /// <inheritdoc/>
    public override bool SupportsParallel => SupportsParallel;

    /// <summary>
    /// Transforms a collection of input records to output records.
    /// </summary>
    /// <param name="source">The source data to transform.</param>
    /// <param name="context">The transformation context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The transformed data.</returns>
    public override IGenericResult<IEnumerable<TOut>> Transform(
        IEnumerable<TIn> source,
        TransformContext context,
        CancellationToken ct)
    {
        var results = new List<TOut>();

        foreach (var input in source)
        {
            if (ct.IsCancellationRequested)
                return GenericResult<IEnumerable<TOut>>.Failure("Transformation cancelled");

            // TODO: Implement transformation logic
            var output = TransformSingle(input, context);
            results.Add(output);
        }

        return GenericResult<IEnumerable<TOut>>.Success(results);
    }

#if (SupportsStreaming)
    /// <summary>
    /// Transforms a single record (for streaming support).
    /// </summary>
    /// <param name="input">The input record.</param>
    /// <param name="context">The transformation context.</param>
    /// <returns>The transformed record.</returns>
    public override TOut TransformSingle(TIn input, TransformContext context)
    {
        // TODO: Implement single record transformation
        throw new System.NotImplementedException("Implement transformation logic");
    }
#endif
}
#else
[TypeOption(typeof(DataTransformers), "DataTransformer")]
public sealed class DataTransformerTransformer : TransformerBase<TInput, TOutput>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataTransformerTransformer"/> class.
    /// </summary>
    public DataTransformerTransformer()
        : base(id: 99, name: "DataTransformer")
    {
    }

    /// <inheritdoc/>
    public override bool SupportsStreaming => SupportsStreaming;

    /// <inheritdoc/>
    public override bool SupportsParallel => SupportsParallel;

    /// <summary>
    /// Transforms a collection of TInput records to TOutput records.
    /// </summary>
    /// <param name="source">The source data to transform.</param>
    /// <param name="context">The transformation context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The transformed data.</returns>
    public override IGenericResult<IEnumerable<TOutput>> Transform(
        IEnumerable<TInput> source,
        TransformContext context,
        CancellationToken ct)
    {
        var results = new List<TOutput>();

        foreach (var input in source)
        {
            if (ct.IsCancellationRequested)
                return GenericResult<IEnumerable<TOutput>>.Failure("Transformation cancelled");

            // TODO: Implement transformation logic
            var output = TransformSingle(input, context);
            results.Add(output);
        }

        return GenericResult<IEnumerable<TOutput>>.Success(results);
    }

#if (SupportsStreaming)
    /// <summary>
    /// Transforms a single record (for streaming support).
    /// </summary>
    /// <param name="input">The input record.</param>
    /// <param name="context">The transformation context.</param>
    /// <returns>The transformed record.</returns>
    public override TOutput TransformSingle(TInput input, TransformContext context)
    {
        // TODO: Implement single record transformation
        // Example:
        // return new TOutput
        // {
        //     Id = input.SourceId,
        //     Name = input.SourceName,
        //     // ... map other properties
        // };
        throw new System.NotImplementedException("Implement transformation logic");
    }
#endif
}
#endif
