using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.EndPoints;

/// <summary>
/// FileConfigurationSource upload, download, and manipulation operations endpoint type.
/// </summary>
[TypeOption(typeof(EndpointTypes), "File")]
public sealed class File : EndpointTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="File"/> class.
    /// </summary>
    public File() : base(
        id: 5,
        name: "FileConfigurationSource",
        description: "FileConfigurationSource upload, download, and manipulation operations",
        defaultHttpMethods: ["GET", "POST", "PUT", "DELETE"],
        requiresAuthentication: true,
        cachingStrategy: "NoCache",
        isReadOnly: false,
        supportsCaching: false,
        defaultCacheDurationSeconds: null,
        requiresValidation: true)
    {
    }
}
