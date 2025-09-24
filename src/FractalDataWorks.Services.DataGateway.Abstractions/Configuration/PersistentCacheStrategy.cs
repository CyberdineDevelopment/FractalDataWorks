using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Configuration;

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