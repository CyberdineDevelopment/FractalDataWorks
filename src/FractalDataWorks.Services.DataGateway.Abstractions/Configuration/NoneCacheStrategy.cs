using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Configuration;

/// <summary>
/// No caching - discover schema on every request.
/// </summary>
/// <remarks>
/// Provides the most up-to-date information but has the highest performance cost.
/// Suitable for development environments or schemas that change frequently.
/// </remarks>
[TypeOption(typeof(CacheStrategies), "None")]
public sealed class NoneCacheStrategy : CacheStrategy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoneCacheStrategy"/> class.
    /// </summary>
    public NoneCacheStrategy() : base(1, "None") { }
}