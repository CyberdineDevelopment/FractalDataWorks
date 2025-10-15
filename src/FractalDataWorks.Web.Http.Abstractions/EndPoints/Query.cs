using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.EndPoints;

/// <summary>
/// Read-only data retrieval operations endpoint type.
/// </summary>
[TypeOption(typeof(EndpointTypes), "Query")]
public sealed class Query : EndpointTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Query"/> class.
    /// </summary>
    public Query() : base(
        id: 2,
        name: "Query",
        description: "Read-only data retrieval and query operations",
        defaultHttpMethods: ["GET"],
        requiresAuthentication: false,
        cachingStrategy: "Cache",
        isReadOnly: true,
        supportsCaching: true,
        defaultCacheDurationSeconds: 300, // 5 minutes
        requiresValidation: false)
    {
    }
}
