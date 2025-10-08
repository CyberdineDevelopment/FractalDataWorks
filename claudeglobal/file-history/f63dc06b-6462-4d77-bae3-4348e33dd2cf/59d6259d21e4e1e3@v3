using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Message indicating that a service lifetime is null.
/// </summary>
[Message("LifetimeNull")]
public sealed class LifetimeNullMessage : FactoryMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LifetimeNullMessage"/> class.
    /// </summary>
    public LifetimeNullMessage()
        : base(3003, "LifetimeNull", MessageSeverity.Error,
               "Lifetime cannot be null", "FACTORY_LIFETIME_NULL") { }
}
