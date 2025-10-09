using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services.Connections.Http.Abstractions.EnhancedEnums.HttpProtocols;

namespace FractalDataWorks.Services.Connections.Rest.EnhancedEnums.Protocols;

/// <summary>
/// REST protocol Enhanced Enum option for HTTP-based REST API connections.
/// </summary>
[EnumOption(typeof(HttpProtocols), "REST")]
public sealed class RestProtocol : HttpProtocolBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestProtocol"/> class.
    /// </summary>
    public RestProtocol() : base(1, "REST", "Representational State Transfer protocol for HTTP-based APIs")
    {
    }
}