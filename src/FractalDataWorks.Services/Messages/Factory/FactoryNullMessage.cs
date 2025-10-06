using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Message indicating that a factory instance is null.
/// </summary>
[Message("FactoryNull")]
public sealed class FactoryNullMessage : FactoryMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FactoryNullMessage"/> class.
    /// </summary>
    public FactoryNullMessage()
        : base(3002, "FactoryNull", MessageSeverity.Error,
               "Factory cannot be null", "FACTORY_NULL") { }
}
