using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.EndPoints;

/// <summary>
/// System health and monitoring endpoints endpoint type.
/// </summary>
[TypeOption(typeof(EndpointTypes), "Health")]
public sealed class Health : EndpointTypeBase
{
    /// <summary>
    /// Gets the description of what this endpoint type represents.
    /// </summary>
    public override string Description => "System health and monitoring endpoints";

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
    public override string CachingStrategy => "NoCache";

    /// <summary>
    /// Gets a value indicating whether this endpoint type is read-only.
    /// </summary>
    public override bool IsReadOnly => true;

    /// <summary>
    /// Gets a value indicating whether this endpoint type supports caching.
    /// </summary>
    public override bool SupportsCaching => false;

    /// <summary>
    /// Gets the default cache duration in seconds for this endpoint type.
    /// </summary>
    public override int? DefaultCacheDurationSeconds => null;

    /// <summary>
    /// Gets a value indicating whether this endpoint type requires validation.
    /// </summary>
    public override bool RequiresValidation => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Health"/> class.
    /// </summary>
    public Health() : base(6, "Health") { }
}
