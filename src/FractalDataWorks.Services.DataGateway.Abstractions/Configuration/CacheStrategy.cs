using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Configuration;

/// <summary>
/// Interface for cache strategies.
/// </summary>
public interface ICacheStrategy : IEnumOption<CacheStrategy>
{
}

/// <summary>
/// Base class for caching strategies for discovered schema information.
/// </summary>
public abstract class CacheStrategy : EnumOptionBase<CacheStrategy>, ICacheStrategy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CacheStrategy"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this cache strategy.</param>
    /// <param name="name">The name of this cache strategy.</param>
    protected CacheStrategy(int id, string name) : base(id, name)
    {
    }
}

/// <summary>
/// No caching - discover schema on every request.
/// </summary>
/// <remarks>
/// Provides the most up-to-date information but has the highest performance cost.
/// Suitable for development environments or schemas that change frequently.
/// </remarks>
[EnumOption("None")]
public sealed class NoneCacheStrategy : CacheStrategy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoneCacheStrategy"/> class.
    /// </summary>
    public NoneCacheStrategy() : base(1, "None") { }
}

/// <summary>
/// Cache schema information in memory for the application lifetime.
/// </summary>
/// <remarks>
/// Fast access to cached information but lost on application restart.
/// Good balance of performance and freshness for most scenarios.
/// </remarks>
[EnumOption("Memory")]
public sealed class MemoryCacheStrategy : CacheStrategy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryCacheStrategy"/> class.
    /// </summary>
    public MemoryCacheStrategy() : base(2, "Memory") { }
}

/// <summary>
/// Cache schema information persistently with configurable expiration.
/// </summary>
/// <remarks>
/// Preserves cache across application restarts. Requires additional storage
/// but provides the best performance for static schemas.
/// </remarks>
[EnumOption("Persistent")]
public sealed class PersistentCacheStrategy : CacheStrategy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PersistentCacheStrategy"/> class.
    /// </summary>
    public PersistentCacheStrategy() : base(3, "Persistent") { }
}

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

/// <summary>
/// Collection of cache strategies.
/// </summary>
[EnumCollection(CollectionName = "CacheStrategies")]
public abstract class CacheStrategyCollectionBase : EnumCollectionBase<CacheStrategy>
{
}
