using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.EndPoints;

/// <summary>
/// Create, Read, Update, Delete operations endpoint type.
/// </summary>
[EnumOption("CRUD")]
public sealed class CRUD : EndpointTypeBase
{
    /// <summary>
    /// Gets the description of what this endpoint type represents.
    /// </summary>
    public override string Description => "Create, Read, Update, Delete operations for data management";

    /// <summary>
    /// Gets the default HTTP methods typically used by this endpoint type.
    /// </summary>
    public override string[] DefaultHttpMethods => ["GET", "POST", "PUT", "DELETE", "PATCH"];

    /// <summary>
    /// Gets a value indicating whether this endpoint type typically requires authentication.
    /// </summary>
    public override bool RequiresAuthentication => true;

    /// <summary>
    /// Gets the recommended caching strategy for this endpoint type.
    /// </summary>
    public override string CachingStrategy => "NoCache";

    /// <summary>
    /// Gets a value indicating whether this endpoint type is read-only.
    /// Read-only endpoints typically don't modify data.
    /// </summary>
    public override bool IsReadOnly => false;

    /// <summary>
    /// Gets a value indicating whether this endpoint type supports caching.
    /// </summary>
    public override bool SupportsCaching => false;

    /// <summary>
    /// Gets the default cache duration in seconds for this endpoint type.
    /// Returns null if caching is not supported.
    /// </summary>
    public override int? DefaultCacheDurationSeconds => null;

    /// <summary>
    /// Gets a value indicating whether this endpoint type requires validation.
    /// </summary>
    public override bool RequiresValidation => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="CRUD"/> class.
    /// </summary>
    public CRUD() : base(1, "CRUD") { }
}
