using FractalDataWorks.Collections;

namespace FractalDataWorks.Web.Http.Abstractions.EndPoints;

/// <summary>
/// Enhanced enum defining different types of endpoints supported by the FractalDataWorks Web Framework.
/// Each type provides semantic meaning and enables framework-specific behavior customization.
/// </summary>
public abstract class EndpointTypeBase : IEndpointType
{
    /// <summary>
    /// Gets the description of what this endpoint type represents.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the default HTTP methods typically used by this endpoint type.
    /// </summary>
    public string[] DefaultHttpMethods { get; }

    /// <summary>
    /// Gets a value indicating whether this endpoint type typically requires authentication.
    /// </summary>
    public bool RequiresAuthentication { get; }

    /// <summary>
    /// Gets the recommended caching strategy for this endpoint type.
    /// </summary>
    public string CachingStrategy { get; }

    /// <summary>
    /// Gets a value indicating whether this endpoint type is read-only.
    /// Read-only endpoints typically don't modify data.
    /// </summary>
    public bool IsReadOnly { get; }

    /// <summary>
    /// Gets a value indicating whether this endpoint type supports caching.
    /// </summary>
    public bool SupportsCaching { get; }

    /// <summary>
    /// Gets the default cache duration in seconds for this endpoint type.
    /// Returns null if caching is not supported.
    /// </summary>
    public int? DefaultCacheDurationSeconds { get; }

    /// <summary>
    /// Gets a value indicating whether this endpoint type requires validation.
    /// </summary>
    public bool RequiresValidation { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this endpoint type.</param>
    /// <param name="name">The name of the endpoint type.</param>
    /// <param name="description">The description of what this endpoint type represents.</param>
    /// <param name="defaultHttpMethods">The default HTTP methods typically used by this endpoint type.</param>
    /// <param name="requiresAuthentication">Indicates whether this endpoint type typically requires authentication.</param>
    /// <param name="cachingStrategy">The recommended caching strategy for this endpoint type.</param>
    /// <param name="isReadOnly">Indicates whether this endpoint type is read-only.</param>
    /// <param name="supportsCaching">Indicates whether this endpoint type supports caching.</param>
    /// <param name="defaultCacheDurationSeconds">The default cache duration in seconds for this endpoint type.</param>
    /// <param name="requiresValidation">Indicates whether this endpoint type requires validation.</param>
    protected EndpointTypeBase(
        int id,
        string name,
        string description,
        string[] defaultHttpMethods,
        bool requiresAuthentication,
        string cachingStrategy,
        bool isReadOnly,
        bool supportsCaching,
        int? defaultCacheDurationSeconds,
        bool requiresValidation)
    {
        Id = id;
        Name = name;
        Description = description;
        DefaultHttpMethods = defaultHttpMethods;
        RequiresAuthentication = requiresAuthentication;
        CachingStrategy = cachingStrategy;
        IsReadOnly = isReadOnly;
        SupportsCaching = supportsCaching;
        DefaultCacheDurationSeconds = defaultCacheDurationSeconds;
        RequiresValidation = requiresValidation;
    }

    /// <inheritdoc/>
    public int Id { get; }

    /// <inheritdoc/>
    public string Name { get; }
}
