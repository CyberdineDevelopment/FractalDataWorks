using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Samples.MsSql;

/// <summary>
/// Sample REST API endpoint path type demonstrating the TypeCollection pattern.
/// </summary>
/// <remarks>
/// In production, this would be in a separate package like FractalDataWorks.Data.Rest
/// and would include actual HTTP endpoint resolution and authentication handling.
/// Included here to demonstrate cross-domain type discovery.
/// </remarks>
[TypeOption(typeof(PathTypes), "RestApi")]
public sealed class RestApiPathType : PathTypeBase
{
    public RestApiPathType()
        : base(
            id: 2,
            name: "RestApi",
            displayName: "REST API Endpoint Path",
            description: "Path to REST API endpoints with HTTP method support",
            domain: "Rest",
            category: "Api")
    {
    }
}
