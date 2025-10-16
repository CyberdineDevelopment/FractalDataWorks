using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Web.Http.Abstractions.EndPoints;

/// <summary>
/// Interface for endpoint type enhanced enums.
/// Provides abstraction for endpoint classification and behavior.
/// </summary>
public interface IEndpointType : IEnumOption
{
    /// <summary>
    /// Gets a value indicating whether this endpoint type is read-only.
    /// Read-only endpoints typically don't modify data.
    /// </summary>
    bool IsReadOnly { get; }
    
    /// <summary>
    /// Gets a value indicating whether this endpoint type supports caching.
    /// </summary>
    bool SupportsCaching { get; }
    
    /// <summary>
    /// Gets the default cache duration in seconds for this endpoint type.
    /// Returns null if caching is not supported.
    /// </summary>
    int? DefaultCacheDurationSeconds { get; }
    
    /// <summary>
    /// Gets a value indicating whether this endpoint type requires validation.
    /// </summary>
    bool RequiresValidation { get; }
}
