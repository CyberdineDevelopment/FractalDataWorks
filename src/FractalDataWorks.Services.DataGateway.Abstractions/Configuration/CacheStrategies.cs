using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Configuration;

/// <summary>
/// Collection of cache strategies.
/// </summary>
[TypeCollection(typeof(CacheStrategy), typeof(ICacheStrategy), typeof(CacheStrategies))]
public sealed partial class CacheStrategies : TypeCollectionBase<CacheStrategy, ICacheStrategy>
{
}