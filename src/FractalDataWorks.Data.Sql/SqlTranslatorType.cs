using System;
using FractalDataWorks.Commands.Abstractions;
using FractalDataWorks.Data.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// SQL translator type definition for LINQ to SQL translation.
/// </summary>
public sealed class SqlTranslatorType : TranslatorTypeBase
{
    /// <summary>
    /// Gets the singleton instance of the SQL translator type.
    /// </summary>
    public static SqlTranslatorType Instance { get; } = new();

    /// <inheritdoc/>
    public override IGenericResult<ICommandTranslator> CreateTranslator(IServiceProvider services)
    {
        var translator = services.GetService(typeof(SqlCommandTranslator)) as SqlCommandTranslator;

        return translator != null
            ? GenericResult.Success<ICommandTranslator>(translator)
            : GenericResult.Failure<ICommandTranslator>(
                Messages.TranslatorNotRegisteredMessage.Instance);
    }

    private SqlTranslatorType()
        : base(
            id: 100,
            name: "SqlTranslator",
            description: "Translates LINQ expressions to T-SQL using Microsoft.SqlServer.TransactSql.ScriptDom",
            sourceFormat: DataFormats.Name("Linq"),
            targetFormat: DataFormats.Name("Sql"),
            capabilities: new TranslationCapabilities
            {
                SupportsProjection = true,
                SupportsFiltering = true,
                SupportsOrdering = true,
                SupportsPaging = true,
                SupportsJoins = true,
                SupportsGrouping = true,
                SupportsAggregation = true,
                SupportsSubqueries = true,
                SupportsTransactions = true,
                SupportsBulkOperations = true,
                SupportsParameterization = true,
                MaxComplexityLevel = 10
            },
            priority: 100)
    {
    }
}