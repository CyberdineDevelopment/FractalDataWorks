using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Connections.Abstractions.Messages;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.Messages;

/// <summary>
/// HTTP connection established message.
/// </summary>
[Message("HttpConnectionEstablished")]
public sealed class HttpConnectionEstablishedMessage : ConnectionMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpConnectionEstablishedMessage"/> class.
    /// </summary>
    public HttpConnectionEstablishedMessage() 
        : base(4002, "HttpConnectionEstablished", MessageSeverity.Information, 
               "HTTP connection established successfully", "HTTP_CONNECTED") { }

    /// <summary>
    /// Initializes a new instance with endpoint details.
    /// </summary>
    /// <param name="endpoint">The endpoint that was connected to.</param>
    public HttpConnectionEstablishedMessage(string endpoint) 
        : base(4002, "HttpConnectionEstablished", MessageSeverity.Information, 
               $"HTTP connection established to {endpoint}", "HTTP_CONNECTED") { }
}