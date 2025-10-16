using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataConceptDemo.Models;
using FractalDataWorks.Data.Transformers.Abstractions;
using FractalDataWorks.Results;

namespace DataConceptDemo.Transformers;

/// <summary>
/// Transforms PayPal payment responses to unified Transaction model.
/// </summary>
public sealed class PayPalTransformer : TransformerBase<PayPalPayment, Transaction>
{
    public PayPalTransformer()
        : base(id: 100, name: "PayPalTransformer")
    {
    }

    public override IGenericResult<IEnumerable<Transaction>> Transform(
        IEnumerable<PayPalPayment> source,
        TransformContext context,
        CancellationToken cancellationToken = default)
    {
        if (source == null)
        {
            return GenericResult<IEnumerable<Transaction>>.Failure("Source cannot be null");
        }

        try
        {
            var transactions = source.Select(payment => new Transaction
            {
                Id = payment.PaymentId,
                Amount = payment.Total,
                Currency = payment.CurrencyCode,
                Date = payment.CreateTime,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.State
            }).ToList();

            return GenericResult<IEnumerable<Transaction>>.Success(transactions);
        }
        catch (Exception ex)
        {
            return GenericResult<IEnumerable<Transaction>>.Failure($"Transform failed: {ex.Message}");
        }
    }
}
