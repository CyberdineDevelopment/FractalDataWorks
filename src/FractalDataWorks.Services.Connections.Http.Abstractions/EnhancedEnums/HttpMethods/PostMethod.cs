using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.EnhancedEnums.HttpMethods;

/// <summary>
/// HTTP POST method - submits data to be processed by the server.
/// </summary>
[EnumOption("Post")]
public sealed class PostMethod : HttpMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostMethod"/> class.
    /// </summary>
    public PostMethod() : base(2, "POST", "Submits data to be processed by the server")
    {
    }
}