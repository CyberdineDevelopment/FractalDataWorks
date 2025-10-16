using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataConceptDemo.Models;
using FractalDataWorks.Data.Transformers.Abstractions;
using FractalDataWorks.Results;

namespace DataConceptDemo.Transformers;

/// <summary>
/// Transforms Stripe charge responses to unified Transaction model.
/// </summary>
public sealed class StripeTransformer : TransformerBase<StripeCharge, Transaction>
{
    public StripeTransformer()
        : base(id: 200, name: "StripeTransformer")
    {
    }

    public override IGenericResult<IEnumerable<Transaction>> Transform(
        IEnumerable<StripeCharge> source,
        TransformContext context,
        CancellationToken cancellationToken = default)
    {
        if (source == null)
        {
            return GenericResult<IEnumerable<Transaction>>.Failure("Source cannot be null");
        }

        try
        {
            var transactions = source.Select(charge => new Transaction
            {
                Id = charge.Id,
                Amount = charge.Amount / 100m, // Convert cents to dollars
                Currency = charge.Currency.ToUpperInvariant(),
                Date = DateTimeOffset.FromUnixTimeSeconds(charge.Created).DateTime,
                PaymentMethod = charge.PaymentMethodType,
                Status = charge.Status
            }).ToList();

            return GenericResult<IEnumerable<Transaction>>.Success(transactions);
        }
        catch (Exception ex)
        {
            return GenericResult<IEnumerable<Transaction>>.Failure($"Transform failed: {ex.Message}");
        }
    }
}
