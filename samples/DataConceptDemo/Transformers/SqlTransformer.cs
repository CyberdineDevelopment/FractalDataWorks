using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataConceptDemo.Models;
using FractalDataWorks.Data.Transformers.Abstractions;
using FractalDataWorks.Results;

namespace DataConceptDemo.Transformers;

/// <summary>
/// Transforms SQL transaction rows to unified Transaction model.
/// </summary>
public sealed class SqlTransformer : TransformerBase<SqlTransaction, Transaction>
{
    public SqlTransformer()
        : base(id: 300, name: "SqlTransformer")
    {
    }

    public override IGenericResult<IEnumerable<Transaction>> Transform(
        IEnumerable<SqlTransaction> source,
        TransformContext context,
        CancellationToken cancellationToken = default)
    {
        if (source == null)
        {
            return GenericResult<IEnumerable<Transaction>>.Failure("Source cannot be null");
        }

        try
        {
            var transactions = source.Select(sqlTx => new Transaction
            {
                Id = sqlTx.TransactionId,
                Amount = sqlTx.Amount,
                Currency = sqlTx.Currency,
                Date = sqlTx.TransactionDate,
                PaymentMethod = sqlTx.Method,
                Status = sqlTx.TransactionStatus
            }).ToList();

            return GenericResult<IEnumerable<Transaction>>.Success(transactions);
        }
        catch (Exception ex)
        {
            return GenericResult<IEnumerable<Transaction>>.Failure($"Transform failed: {ex.Message}");
        }
    }
}
