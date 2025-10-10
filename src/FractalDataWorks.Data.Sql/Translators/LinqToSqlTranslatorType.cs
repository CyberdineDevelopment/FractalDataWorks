using System;
using FractalDataWorks.Commands.Abstractions;
using FractalDataWorks.Data.Abstractions;
using FractalDataWorks.Data.Sql.Formats;
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
    public override IDataFormat SourceFormat => SqlDataFormat.Instance; // Temporarily use SQL format until LinqFormat is created

    /// <inheritdoc/>
    public override IDataFormat TargetFormat => SqlDataFormat.Instance;

    /// <inheritdoc/>
    public override TranslationCapabilities Capabilities => new TranslationCapabilities
    {
        SupportsProjection = true,
        SupportsFiltering = true,
        SupportsOrdering = true,
        SupportsGrouping = true,
        SupportsJoins = true,
        SupportsAggregation = true,
        SupportsSubqueries = true,
        SupportsPaging = true,
        SupportsTransactions = true,
        SupportsBulkOperations = true,
        SupportsParameterization = true,
        MaxComplexityLevel = 10
    };

    /// <inheritdoc/>
    public override int Priority => 100;

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
        : base(1, "LinqToSql", "Translates LINQ expression trees to T-SQL queries")
    {
    }
}