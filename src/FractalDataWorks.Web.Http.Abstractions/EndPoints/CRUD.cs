using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.EndPoints;

/// <summary>
/// Create, Read, Update, Delete operations endpoint type.
/// </summary>
[TypeOption(typeof(EndpointTypes), "CRUD")]
public sealed class CRUD : EndpointTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CRUD"/> class.
    /// </summary>
    public CRUD() : base(
        id: 1,
        name: "CRUD",
        description: "Create, Read, Update, Delete operations for data management",
        defaultHttpMethods: ["GET", "POST", "PUT", "DELETE", "PATCH"],
        requiresAuthentication: true,
        cachingStrategy: "NoCache",
        isReadOnly: false,
        supportsCaching: false,
        defaultCacheDurationSeconds: null,
        requiresValidation: true)
    {
    }
}
