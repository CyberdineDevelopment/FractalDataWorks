using System.Globalization;
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Message indicating that FastGeneric service instantiation failed.
/// The source generator will create FactoryMessages.FastGenericCreationFailed(serviceType) method.
/// </summary>
[Message("FastGenericCreationFailed")]
public sealed class FastGenericCreationFailedMessage : FactoryMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FastGenericCreationFailedMessage"/> class with default message template.
    /// </summary>
    public FastGenericCreationFailedMessage() 
        : base(2002, "FastGenericCreationFailed", MessageSeverity.Error, 
               "FastGeneric failed to create service of type {0}", "FASTGENERIC_CREATION_FAILED") { }

    /// <summary>
    /// Initializes a new instance with the service type that failed FastGeneric creation.
    /// </summary>
    /// <param name="serviceType">The type of service that FastGeneric failed to create.</param>
    public FastGenericCreationFailedMessage(string serviceType)
        : base(2002, "FastGenericCreationFailed", MessageSeverity.Error, 
               string.Format(CultureInfo.InvariantCulture, "FastGeneric failed to create service of type {0}", serviceType), 
               "FASTGENERIC_CREATION_FAILED") { }

}