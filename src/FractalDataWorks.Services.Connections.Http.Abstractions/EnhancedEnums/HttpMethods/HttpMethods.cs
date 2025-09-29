using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.EnhancedEnums.HttpMethods;

/// <summary>
/// Collection of HTTP methods for enhanced enum functionality.
/// Source generator creates static HttpMethods class automatically.
/// </summary>
[TypeCollection(typeof(HttpMethodBase), typeof(IHttpMethod), typeof(HttpMethods))]
public abstract class HttpMethods : HttpMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpMethods"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the HTTP method.</param>
    /// <param name="name">The name of the HTTP method.</param>
    /// <param name="description">The description of the HTTP method.</param>
    protected HttpMethods(int id, string name, string description) : base(id, name, description)
    {
    }
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