using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Web.Http.Abstractions.EndPoints;

/// <summary>
/// Enhanced enum defining different types of endpoints supported by the FractalDataWorks Web Framework.
/// Each type provides semantic meaning and enables framework-specific behavior customization.
/// </summary>
public abstract class EndpointTypeBase : EnumOptionBase<EndpointTypeBase>, IEndpointType, IEnumOption<EndpointTypeBase>
{
    /// <summary>
    /// Gets the description of what this endpoint type represents.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Gets the default HTTP methods typically used by this endpoint type.
    /// </summary>
    public abstract string[] DefaultHttpMethods { get; }

    /// <summary>
    /// Gets a value indicating whether this endpoint type typically requires authentication.
    /// </summary>
    public abstract bool RequiresAuthentication { get; }

    /// <summary>
    /// Gets the recommended caching strategy for this endpoint type.
    /// </summary>
    public abstract string CachingStrategy { get; }

    /// <summary>
    /// Gets a value indicating whether this endpoint type is read-only.
    /// Read-only endpoints typically don't modify data.
    /// </summary>
    public abstract bool IsReadOnly { get; }

    /// <summary>
    /// Gets a value indicating whether this endpoint type supports caching.
    /// </summary>
    public abstract bool SupportsCaching { get; }

    /// <summary>
    /// Gets the default cache duration in seconds for this endpoint type.
    /// Returns null if caching is not supported.
    /// </summary>
    public abstract int? DefaultCacheDurationSeconds { get; }

    /// <summary>
    /// Gets a value indicating whether this endpoint type requires validation.
    /// </summary>
    public abstract bool RequiresValidation { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this endpoint type.</param>
    /// <param name="name">The name of the endpoint type.</param>
    protected EndpointTypeBase(int id, string name) : base(id, name) { }
}
