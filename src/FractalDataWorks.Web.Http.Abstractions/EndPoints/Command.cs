using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.EndPoints;

/// <summary>
/// State-changing business operations endpoint type.
/// </summary>
[TypeOption(typeof(EndpointTypes), "Command")]
public sealed class Command : EndpointTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Command"/> class.
    /// </summary>
    public Command() : base(
        id: 3,
        name: "Command",
        description: "State-changing business operations and actions",
        defaultHttpMethods: ["POST", "PUT", "PATCH"],
        requiresAuthentication: true,
        cachingStrategy: "NoCache",
        isReadOnly: false,
        supportsCaching: false,
        defaultCacheDurationSeconds: null,
        requiresValidation: true)
    {
    }
}
