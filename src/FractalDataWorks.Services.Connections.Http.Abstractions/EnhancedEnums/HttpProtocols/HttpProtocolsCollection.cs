using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.EnhancedEnums.HttpProtocols;

/// <summary>
/// Collection of HTTP protocols for enhanced enum functionality.
/// Source generator creates static HttpProtocols class automatically.
/// </summary>
[StaticEnumCollection(CollectionName = "HttpProtocols", DefaultGenericReturnType = typeof(IHttpProtocol))]
public abstract class HttpProtocolsCollection : EnumCollectionBase<HttpProtocolBase>
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