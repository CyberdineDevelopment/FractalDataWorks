using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Configuration;

/// <summary>
/// Use memory cache with persistent backup for resilience.
/// </summary>
/// <remarks>
/// Combines the speed of memory cache with the durability of persistent cache.
/// Memory cache is populated from persistent cache on startup if available.
/// </remarks>
[EnumOption("Hybrid")]
public sealed class HybridCacheStrategy : CacheStrategy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HybridCacheStrategy"/> class.
    /// </summary>
    public HybridCacheStrategy() : base(4, "Hybrid") { }
}