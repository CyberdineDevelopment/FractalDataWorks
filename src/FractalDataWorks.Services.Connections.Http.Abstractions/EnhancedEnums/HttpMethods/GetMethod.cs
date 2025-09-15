using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.EnhancedEnums.HttpMethods;

/// <summary>
/// HTTP GET method - retrieves data from the server.
/// </summary>
[EnumOption("Get")]
public sealed class GetMethod : HttpMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetMethod"/> class.
    /// </summary>
    public GetMethod() : base(1, "GET", "Retrieves data from the server")
    {
    }
}