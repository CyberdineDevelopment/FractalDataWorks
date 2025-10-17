using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.EnhancedEnums.HttpMethods;

/// <summary>
/// Base class for HTTP method types in the Enhanced Enum pattern.
/// </summary>
public abstract class HttpMethodBase : EnumOptionBase<IHttpMethod>, IEnumOption<HttpMethodBase>, IHttpMethod
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpMethodBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the HTTP method.</param>
    /// <param name="name">The name of the HTTP method.</param>
    /// <param name="description">The description of the HTTP method.</param>
    protected HttpMethodBase(int id, string name, string description) 
        : base(id, name)
    {
        Description = description;
    }

    /// <summary>
    /// Gets the description of the HTTP method.
    /// </summary>
    public string Description { get; }
}