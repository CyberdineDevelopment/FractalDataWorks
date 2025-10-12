using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.EnhancedEnums.HttpMethods;

/// <summary>
/// Source generator creates static HttpMethods class automatically.
/// </summary>
[TypeCollection(typeof(HttpMethodBase), typeof(IHttpMethod), typeof(HttpMethods))]
public abstract partial class HttpMethods : TypeCollectionBase<HttpMethodBase,IHttpMethod>
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