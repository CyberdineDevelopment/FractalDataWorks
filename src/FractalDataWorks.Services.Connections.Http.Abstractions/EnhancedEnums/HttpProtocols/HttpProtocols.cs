using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.EnhancedEnums.HttpProtocols;

/// <summary>
/// Collection of HTTP protocols for enhanced enum functionality.
/// Source generator creates static HttpProtocols class automatically.
/// </summary>
[TypeCollection(typeof(HttpProtocolBase), typeof(IHttpProtocol), typeof(HttpProtocols))]
public abstract partial class HttpProtocols : TypeCollectionBase<HttpProtocolBase,IHttpProtocol>
{
    // DO NOT IMPLEMENT BY HAND!
    // Source generator automatically creates static HttpProtocols class with:
    // - HttpProtocols.Rest (returns IHttpProtocol)
    // - HttpProtocols.Soap (returns IHttpProtocol)
    // - HttpProtocols.GraphQL (returns IHttpProtocol)
    // - HttpProtocols.All (collection of IHttpProtocol)
    // - HttpProtocols.GetById(int id) (returns IHttpProtocol)
    // - HttpProtocols.GetByName(string name) (returns IHttpProtocol)
}