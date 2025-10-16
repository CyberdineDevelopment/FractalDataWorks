using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.EndPoints;

/// <summary>
/// System health and monitoring endpoints endpoint type.
/// </summary>
[TypeOption(typeof(EndpointTypes), "Health")]
public sealed class Health : EndpointTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Health"/> class.
    /// </summary>
    public Health() : base(
        id: 6,
        name: "Health",
        description: "System health and monitoring endpoints",
        defaultHttpMethods: ["GET"],
        requiresAuthentication: false,
        cachingStrategy: "NoCache",
        isReadOnly: true,
        supportsCaching: false,
        defaultCacheDurationSeconds: null,
        requiresValidation: false)
    {
    }
}
