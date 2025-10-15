using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.EndPoints;

/// <summary>
/// Event publishing and subscription operations endpoint type.
/// </summary>
[TypeOption(typeof(EndpointTypes), "EventEndpoint")]
public sealed class EventEndpoint : EndpointTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventEndpoint"/> class.
    /// </summary>
    public EventEndpoint() : base(
        id: 4,
        name: "EventEndpoint",
        description: "Event publishing and subscription operations",
        defaultHttpMethods: ["POST", "GET"],
        requiresAuthentication: true,
        cachingStrategy: "NoCache",
        isReadOnly: false,
        supportsCaching: false,
        defaultCacheDurationSeconds: null,
        requiresValidation: true)
    {
    }
}
