using System;

namespace DataConceptDemo.Models;

/// <summary>
/// Represents a transaction row from SQL database.
/// This is a simplified model of what a database might store.
/// </summary>
public sealed class SqlTransaction
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime TransactionDate { get; set; }
    public string Method { get; set; } = string.Empty;
    public string TransactionStatus { get; set; } = string.Empty;
}
