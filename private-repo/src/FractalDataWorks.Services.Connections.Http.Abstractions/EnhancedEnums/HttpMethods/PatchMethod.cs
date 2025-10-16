using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.EnhancedEnums.HttpMethods;

/// <summary>
/// HTTP PATCH method - applies partial modifications to a resource.
/// </summary>
[TypeOption(typeof(HttpMethodCollection), "Patch")]
public sealed class PatchMethod : HttpMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PatchMethod"/> class.
    /// </summary>
    public PatchMethod() : base(5, "PATCH", "Applies partial modifications to a resource")
    {
    }
}