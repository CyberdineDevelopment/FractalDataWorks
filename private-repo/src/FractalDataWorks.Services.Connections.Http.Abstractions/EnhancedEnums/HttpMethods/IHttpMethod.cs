using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.EnhancedEnums.HttpMethods;

/// <summary>
/// Interface defining the contract for HTTP method enum options.
/// </summary>
public interface IHttpMethod : IEnumOption<IHttpMethod>
{
    /// <summary>
    /// Gets the description of this HTTP method.
    /// </summary>
    string Description { get; }
}