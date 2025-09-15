using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.EndPoints;

/// <summary>
/// FileConfigurationSource upload, download, and manipulation operations endpoint type.
/// </summary>
[EnumOption("FileConfigurationSource")]
public sealed class File : EndpointTypeBase
{
    /// <summary>
    /// Gets the description of what this endpoint type represents.
    /// </summary>
    public override string Description => "FileConfigurationSource upload, download, and manipulation operations";

    /// <summary>
    /// Gets the default HTTP methods typically used by this endpoint type.
    /// </summary>
    public override string[] DefaultHttpMethods => ["GET", "POST", "PUT", "DELETE"];

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
    /// </summary>
    public override bool IsReadOnly => false;

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
    public override bool RequiresValidation => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="File"/> class.
    /// </summary>
    public File() : base(5, "FileConfigurationSource") { }
}
