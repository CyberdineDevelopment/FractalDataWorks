using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.EnhancedEnums.HttpMethods;

/// <summary>
/// HTTP DELETE method - deletes a resource from the server.
/// </summary>
[TypeOption(typeof(HttpMethods), "Delete")]
public sealed class DeleteMethod : HttpMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteMethod"/> class.
    /// </summary>
    public DeleteMethod() : base(4, "DELETE", "Deletes a resource from the server")
    {
    }
}