using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataConceptDemo.Models;
using FractalDataWorks.Data.Transformers.Abstractions;
using FractalDataWorks.Results;

namespace DataConceptDemo.Transformers;

/// <summary>
/// Mock transformer for Milestone 1 demonstration.
/// In Milestone 2, we'll create real transformers with [TypeOption] attributes.
/// </summary>
public sealed class MockTransformer : TransformerBase<object, Transaction>
{
    public MockTransformer()
        : base(id: 1, name: "MockTransformer")
    {
    }

    public override IGenericResult<IEnumerable<Transaction>> Transform(
        IEnumerable<object> source,
        TransformContext context,
        CancellationToken cancellationToken = default)
    {
        // For demo purposes, create some mock transactions
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                Id = "TX001",
                Amount = 99.99m,
                Currency = "USD",
                Date = DateTime.UtcNow.AddDays(-1),
                PaymentMethod = "Mock",
                Status = "Completed",
                CustomerId = "CUST001",
                Description = "Mock transaction from transformer"
            },
            new Transaction
            {
                Id = "TX002",
                Amount = 149.50m,
                Currency = "USD",
                Date = DateTime.UtcNow.AddDays(-2),
                PaymentMethod = "Mock",
                Status = "Completed",
                CustomerId = "CUST002",
                Description = "Another mock transaction"
            }
        };

        return GenericResult<IEnumerable<Transaction>>.Success(transactions);
    }
}
