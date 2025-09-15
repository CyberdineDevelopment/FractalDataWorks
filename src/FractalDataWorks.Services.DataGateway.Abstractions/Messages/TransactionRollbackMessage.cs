using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Messages;

/// <summary>
/// Message indicating that a transaction was rolled back.
/// </summary>
[Message("TransactionRollback")]
public sealed class TransactionRollbackMessage : DataGatewayMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionRollbackMessage"/> class.
    /// </summary>
    public TransactionRollbackMessage() 
        : base(3003, "TransactionRollback", MessageSeverity.Warning, 
               "Transaction was rolled back", "DATA_TRANSACTION_ROLLBACK") { }

    /// <summary>
    /// Initializes a new instance with rollback reason.
    /// </summary>
    /// <param name="reason">The reason for the rollback.</param>
    public TransactionRollbackMessage(string reason) 
        : base(3003, "TransactionRollback", MessageSeverity.Warning, 
               $"Transaction was rolled back: {reason}", "DATA_TRANSACTION_ROLLBACK") { }
}