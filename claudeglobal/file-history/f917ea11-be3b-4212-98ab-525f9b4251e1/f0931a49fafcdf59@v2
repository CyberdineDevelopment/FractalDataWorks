using FractalDataWorks.Messages;

namespace FractalDataWorks.Services.DomainName.Abstractions.Messages;

/// <summary>
/// Message indicating that the requested service type is unknown.
/// </summary>
public sealed class UnknownServiceTypeMessage : DomainNameMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownServiceTypeMessage"/> class.
    /// </summary>
    public UnknownServiceTypeMessage(string serviceType)
        : base(
            id: 1002,
            name: "UnknownServiceType",
            severity: MessageSeverity.Error,
            message: $"Unknown DomainName service type: {serviceType}",
            code: "DOMAIN_UNKNOWN_TYPE",
            source: "DomainNameProvider",
            details: $"Service type '{serviceType}' is not registered in DomainNameTypes collection")
    {
    }
}
