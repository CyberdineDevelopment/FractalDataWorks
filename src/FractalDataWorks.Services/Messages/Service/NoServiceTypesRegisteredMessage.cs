using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// CurrentMessage indicating that no service types have been registered.
/// </summary>
[Message("NoServiceTypesRegistered")]
public sealed class NoServiceTypesRegisteredMessage : ServiceMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoServiceTypesRegisteredMessage"/> class.
    /// </summary>
    public NoServiceTypesRegisteredMessage() 
        : base(1005, "NoServiceTypesRegistered", MessageSeverity.Error, 
               "No service types registered", "NO_SERVICE_TYPES") { }

}
