using System;
using FractalDataWorks.Commands.Abstractions;
using FractalDataWorks.Data.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Data.Sql.Translators;

/// <summary>
/// Translator type for converting LINQ expressions to SQL queries.
/// </summary>
/// <remarks>
/// Provides metadata and factory methods for creating LINQ to SQL translators
/// that convert expression trees into T-SQL queries using Microsoft.SqlServer.TransactSql.ScriptDom.
/// </remarks>
public sealed class LinqToSqlTranslatorType : TranslatorTypeBase
{
    /// <summary>
    /// Gets the singleton instance of the LINQ to SQL translator type.
    /// </summary>
    public static LinqToSqlTranslatorType Instance { get; } = new();

    /// <inheritdoc/>
    public override IGenericResult<ICommandTranslator> CreateTranslator(IServiceProvider services)
    {
        try
        {
            // In a real implementation, this would resolve dependencies from DI
            var translator = new LinqToSqlTranslator();
            return GenericResult<ICommandTranslator>.Success(translator);
        }
        catch (Exception ex)
        {
            return GenericResult<ICommandTranslator>.Failure($"Failed to create LINQ to SQL translator: {ex.Message}");
        }
    }

    internal LinqToSqlTranslatorType()
        : base(
            id: 1,
            name: "LinqToSql",
            description: "Translates LINQ expression trees to T-SQL queries",
            sourceFormat: SqlDataFormat.Instance, // Temporarily use SQL format until LinqFormat is created
            targetFormat: SqlDataFormat.Instance,
            capabilities: new TranslationCapabilities
            {
                SupportsProjection = true,
                SupportsFiltering = true,
                SupportsOrdering = true,
                SupportsGrouping = true,
                SupportsJoins = true,
                SupportsAggregation = true,
                SupportsPaging = true,
                SupportsTransactions = true,
                SupportsBulkOperations = true,
                SupportsParameterization = true,
                MaxComplexityLevel = 10
            },
            priority: 100)
    {
    }
}
