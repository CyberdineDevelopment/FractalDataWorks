using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.EnhancedEnums.HttpMethods;

/// <summary>
/// Collection of HTTP methods for enhanced enum functionality.
/// Source generator creates static HttpMethods class automatically.
/// </summary>
[TypeCollection(typeof(HttpMethodBase), typeof(IHttpMethod), typeof(HttpMethods))]
public abstract class HttpMethods : HttpMethodBase
{
    // DO NOT IMPLEMENT BY HAND!
    // Source generator automatically creates static HttpMethods class with:
    // - HttpMethods.Get (returns IHttpMethod)
    // - HttpMethods.Post (returns IHttpMethod)
    // - HttpMethods.Put (returns IHttpMethod)
    // - HttpMethods.Delete (returns IHttpMethod)
    // - HttpMethods.Patch (returns IHttpMethod)
    // - HttpMethods.All (collection of IHttpMethod)
    // - HttpMethods.GetById(int id) (returns IHttpMethod)
    // - HttpMethods.GetByName(string name) (returns IHttpMethod)
}