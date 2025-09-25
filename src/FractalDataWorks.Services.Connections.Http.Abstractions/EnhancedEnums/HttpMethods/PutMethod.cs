using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.EnhancedEnums.HttpMethods;

/// <summary>
/// HTTP PUT method - uploads or replaces a resource on the server.
/// </summary>
[TypeOption(typeof(HttpMethods), "Put")]
public sealed class PutMethod : HttpMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PutMethod"/> class.
    /// </summary>
    public PutMethod() : base(3, "PUT", "Uploads or replaces a resource on the server")
    {
    }
}