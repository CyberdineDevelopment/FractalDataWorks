using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.EnhancedEnums;

/// <summary>
/// Base class for HTTP protocol types in the Enhanced Enum pattern.
/// </summary>
public abstract class HttpProtocolBase : EnumOptionBase<IHttpProtocol>, IEnumOption<HttpProtocolBase>, IHttpProtocol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpProtocolBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the HTTP protocol.</param>
    /// <param name="name">The name of the HTTP protocol.</param>
    /// <param name="description">The description of the HTTP protocol.</param>
    protected HttpProtocolBase(int id, string name, string description) 
        : base(id, name)
    {
        Description = description;
    }

    /// <summary>
    /// Gets the description of the HTTP protocol.
    /// </summary>
    public string Description { get; }
}