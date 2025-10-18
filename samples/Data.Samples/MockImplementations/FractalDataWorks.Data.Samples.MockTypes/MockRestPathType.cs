using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Samples.MockTypes;

/// <summary>
/// Demonstrates the PathType TypeCollection pattern for REST API endpoints.
/// </summary>
/// <remarks>
/// <para><strong>What This Demonstrates:</strong></para>
/// <list type="bullet">
///   <item><description>
///     <strong>Multiple Types in Same Collection:</strong> PathTypes can contain both SQL and REST
///     path types, showing how TypeCollections aggregate heterogeneous implementations.
///   </description></item>
///   <item><description>
///     <strong>Domain Segregation:</strong> The "Rest" domain distinguishes REST paths from SQL paths,
///     enabling domain-specific logic without switch statements.
///   </description></item>
///   <item><description>
///     <strong>Cross-Assembly Discovery:</strong> This mock type can be discovered alongside production
///     path types from other assemblies, demonstrating the extensibility of the pattern.
///   </description></item>
///   <item><description>
///     <strong>Runtime Resolution:</strong> Configuration files use "RestApi" name to select this type
///     without knowing the implementing class name.
///   </description></item>
/// </list>
/// <para><strong>In Production Use:</strong></para>
/// <para>
/// Configuration would reference this as:
/// <code>
/// "DataConnection": {
///   "PathType": "RestApi",  // Resolved to this type at runtime
///   "Path": "https://api.example.com/v1/customers"
/// }
/// </code>
/// </para>
/// </remarks>
[TypeOption(typeof(PathTypes), "RestApi")]
public sealed class MockRestPathType : PathTypeBase
{
    public MockRestPathType()
        : base(
            id: 2,
            name: "RestApi",
            displayName: "REST API Endpoint Path",
            description: "Path to REST API endpoints",
            domain: "Rest")
    {
    }
}
