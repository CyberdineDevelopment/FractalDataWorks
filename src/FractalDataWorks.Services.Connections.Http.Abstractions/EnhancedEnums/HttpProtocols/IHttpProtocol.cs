using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.EnhancedEnums.HttpProtocols;

/// <summary>
/// Interface defining the contract for HTTP protocol enum options.
/// </summary>
public interface IHttpProtocol : IEnumOption<IHttpProtocol>
{
    /// <summary>
    /// Gets the description of this HTTP protocol.
    /// </summary>
    string Description { get; }
}