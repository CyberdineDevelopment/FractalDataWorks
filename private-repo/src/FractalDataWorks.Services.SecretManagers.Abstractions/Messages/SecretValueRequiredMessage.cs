using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.SecretManagers.Abstractions.Messages;

/// <summary>
/// CurrentMessage indicating that SecretValue parameter is required for an operation.
/// </summary>
[Message("SecretValueRequired")]
public sealed class SecretValueRequiredMessage : SecretManagerMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretValueRequiredMessage"/> class.
    /// </summary>
    /// <param name="operation">The operation that requires the secret value.</param>
    public SecretValueRequiredMessage(string operation)
        : base(1003, "SecretValueRequired", MessageSeverity.Error,
               $"SecretValue parameter is required for {operation} operation.", "SM_VALUE_REQUIRED") { }
}
