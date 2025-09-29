using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.DataGateway.Abstractions;

/// <summary>
/// Collection of cache strategies.
/// </summary>
[TypeCollection(typeof(CacheStrategy), typeof(ICacheStrategy), typeof(CacheStrategies))]
public abstract partial class CacheStrategies : TypeCollectionBase<CacheStrategy, ICacheStrategy>
{
}