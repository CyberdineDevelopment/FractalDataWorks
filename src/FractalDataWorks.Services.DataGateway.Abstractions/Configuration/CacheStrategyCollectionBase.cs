using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Configuration;

/// <summary>
/// Collection of cache strategies.
/// </summary>
[EnumCollection(CollectionName = "CacheStrategies")]
public abstract class CacheStrategyCollectionBase : EnumCollectionBase<CacheStrategy>
{
}