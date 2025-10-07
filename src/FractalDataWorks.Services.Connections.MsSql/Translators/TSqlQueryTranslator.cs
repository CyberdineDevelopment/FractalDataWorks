using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using FractalDataWorks.DataSets.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions.Messages;


namespace FractalDataWorks.Services.Connections.MsSql.Translators;

/// <summary>
/// Translates LINQ expressions into T-SQL commands for SQL Server.
/// Handles SQL Server specific syntax, data types, and query capabilities.
/// </summary>
/// <remarks>
/// This translator converts universal LINQ queries into executable SqlCommand objects
/// that can be run against SQL Server databases. It supports the core LINQ operations
/// including Where, Select, OrderBy, Take, Skip, and basic aggregations while respecting
/// SQL Server's syntax and limitations.
/// </remarks>
internal sealed class TSqlQueryTranslator : IQueryTranslator
{
    private const int DefaultComplexityLimit = 50;
    private const int MaxParameterCount = 2000; // SQL Server limit is 2100

    /// <inheritdoc/>
    public string ConnectionType => "MsSql";

    /// <inheritdoc/>
    public IEnumerable<string> SupportedContainerTypes => new[]
    {
        "SqlTable",
        "SqlView",
        "SqlStoredProcedure"
    };

    /// <inheritdoc/>
    public async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataQuery query,
        IDataSetType dataSet,
        string containerType)
    {
        if (query == null)
            return GenericResult<IConnectionCommand>.Failure("Query cannot be null");
        if (dataSet == null)
            return GenericResult<IConnectionCommand>.Failure("DataSet cannot be null");

        try
        {
            // Validate query first
            var validation = await ValidateQueryAsync(query, dataSet, containerType).ConfigureAwait(false);
            if (!validation.IsSuccess)
            {
                return GenericResult<IConnectionCommand>.Failure(validation.ErrorMessage!);
            }

            // Build T-SQL command
            var builder = new TSqlCommandBuilder(dataSet, containerType);
            var sqlCommand = await builder.BuildAsync(query).ConfigureAwait(false);

            if (!sqlCommand.IsSuccess)
            {
                return GenericResult<IConnectionCommand>.Failure($"Failed to build T-SQL command: {sqlCommand.ErrorMessage}");
            }

            // Create connection command wrapper
            var connectionCommand = new SqlConnectionCommand(sqlCommand.Value!);
            return GenericResult<IConnectionCommand>.Success(connectionCommand);
        }
        catch (Exception ex)
        {
            return GenericResult<IConnectionCommand>.Failure($"Translation failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<IGenericResult> ValidateQueryAsync(
        IDataQuery query,
        IDataSetType dataSet,
        string containerType)
    {
        if (query == null)
            return GenericResult.Failure(ConnectionMessages.QueryNull());
        if (dataSet == null)
            return GenericResult.Failure(ConnectionMessages.DataSetNull());

        try
        {
            // Check container type support
            if (!SupportedContainerTypes.Contains(containerType))
            {
                return GenericResult.Failure($"Container type '{containerType}' is not supported by T-SQL translator");
            }

            // Validate query expression complexity
            var complexity = CalculateQueryComplexity(query.Expression);
            if (complexity > DefaultComplexityLimit)
            {
                return GenericResult.Failure($"Query complexity ({complexity}) exceeds maximum allowed ({DefaultComplexityLimit})");
            }

            // Check for unsupported operations
            var visitor = new UnsupportedOperationVisitor();
            visitor.Visit(query.Expression);
            
            if (visitor.UnsupportedOperations.Count > 0)
            {
                var operations = string.Join(", ", visitor.UnsupportedOperations);
                return GenericResult.Failure($"Query contains unsupported operations: {operations}");
            }

            // Estimate parameter count
            var parameterCount = EstimateParameterCount(query.Expression);
            if (parameterCount > MaxParameterCount)
            {
                return GenericResult.Failure($"Query would generate too many parameters ({parameterCount}), maximum allowed is {MaxParameterCount}");
            }

            return await Task.FromResult(GenericResult.Success()).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return GenericResult.Failure($"Query validation failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public TranslatorCapabilities GetCapabilities()
    {
        return new TranslatorCapabilities(
            supportedOperations: new[]
            {
                "Where", "Select", "OrderBy", "OrderByDescending",
                "ThenBy", "ThenByDescending", "Take", "Skip",
                "First", "FirstOrDefault", "Single", "SingleOrDefault",
                "Count", "Sum", "Average", "Min", "Max",
                "GroupBy", "Distinct"
            },
            maxComplexity: DefaultComplexityLimit,
            supportsJoins: true,
            supportsAggregation: true,
            limitations: new[]
            {
                "Complex nested queries may hit complexity limits",
                "String operations limited to T-SQL functions",
                "Date operations limited to T-SQL DATEADD/DATEDIFF",
                "Maximum 2000 query parameters"
            }
        );
    }

    private static int CalculateQueryComplexity(Expression expression)
    {
        var visitor = new ComplexityCountingVisitor();
        visitor.Visit(expression);
        return visitor.Complexity;
    }

    private static int EstimateParameterCount(Expression expression)
    {
        var visitor = new ParameterCountingVisitor();
        visitor.Visit(expression);
        return visitor.ParameterCount;
    }

    /// <summary>
    /// Visitor that detects unsupported LINQ operations for T-SQL translation.
    /// </summary>
    private sealed class UnsupportedOperationVisitor : ExpressionVisitor
    {
        public List<string> UnsupportedOperations { get; } = new List<string>();

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // Check for unsupported methods
            if (node.Method.DeclaringType == typeof(Enumerable) || 
                node.Method.DeclaringType == typeof(Queryable))
            {
                switch (node.Method.Name)
                {
                    case "Zip":
                    case "Concat": 
                    case "Union":
                    case "Intersect":
                    case "Except":
                        UnsupportedOperations.Add(node.Method.Name);
                        break;
                }
            }

            return base.VisitMethodCall(node);
        }
    }

    /// <summary>
    /// Visitor that calculates the complexity score of a LINQ expression.
    /// </summary>
    private sealed class ComplexityCountingVisitor : ExpressionVisitor
    {
        public int Complexity { get; private set; }

        public override Expression Visit(Expression? node)
        {
            if (node != null)
            {
                Complexity++;
            }
            return base.Visit(node)!;
        }
    }

    /// <summary>
    /// Visitor that estimates how many SQL parameters will be needed.
    /// </summary>
    private sealed class ParameterCountingVisitor : ExpressionVisitor
    {
        public int ParameterCount { get; private set; }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            // Each constant value becomes a parameter
            if (node.Value != null && node.Type != typeof(IQueryable))
            {
                ParameterCount++;
            }
            return base.VisitConstant(node);
        }
    }
}


