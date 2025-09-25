using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.EndPoints;

/// <summary>
/// Read-only data retrieval operations endpoint type.
/// </summary>
[TypeOption(typeof(EndpointTypes), "Query")]
public sealed class Query : EndpointTypeBase
{
    /// <summary>
    /// Gets the description of what this endpoint type represents.
    /// </summary>
    public override string Description => "Read-only data retrieval and query operations";

    /// <summary>
    /// Gets the default HTTP methods typically used by this endpoint type.
    /// </summary>
    public override string[] DefaultHttpMethods => ["GET"];

    /// <summary>
    /// Gets a value indicating whether this endpoint type typically requires authentication.
    /// </summary>
    public override bool RequiresAuthentication => false;

    /// <summary>
    /// Gets the recommended caching strategy for this endpoint type.
    /// </summary>
    public override string CachingStrategy => "Cache";

    /// <summary>
    /// Gets a value indicating whether this endpoint type is read-only.
    /// Read-only endpoints typically don't modify data.
    /// </summary>
    public override bool IsReadOnly => true;

    /// <summary>
    /// Gets a value indicating whether this endpoint type supports caching.
    /// </summary>
    public override bool SupportsCaching => true;

    /// <summary>
    /// Gets the default cache duration in seconds for this endpoint type.
    /// Returns null if caching is not supported.
    /// </summary>
    public override int? DefaultCacheDurationSeconds => 300; // 5 minutes

    /// <summary>
    /// Gets a value indicating whether this endpoint type requires validation.
    /// </summary>
    public override bool RequiresValidation => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Query"/> class.
    /// </summary>
    public Query() : base(2, "Query") { }
}
