using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace FractalDataWorks.Data.Sql.Translators;

/// <summary>
/// Visits LINQ expressions and converts them to T-SQL fragments.
/// </summary>
public sealed class LinqToTSqlVisitor : ExpressionVisitor
{
    private readonly Stack<TSqlFragment> _fragments = new();
    private readonly List<string> _errors = new();
    private SelectStatement? _currentSelect;
    private QuerySpecification? _currentQuery;

    /// <summary>
    /// Determines if this visitor can translate the given expression.
    /// </summary>
    public bool CanTranslate(Expression expression)
    {
        _errors.Clear();

        try
        {
            Visit(expression);
            return _errors.Count == 0;
        }
        catch
        {
            return false;
        }
        finally
        {
            _fragments.Clear();
            _currentSelect = null;
            _currentQuery = null;
        }
    }

    /// <summary>
    /// Visits an expression and returns the resulting T-SQL fragment.
    /// </summary>
    public TSqlFragment? Visit(Expression expression)
    {
        _errors.Clear();
        _fragments.Clear();
        _currentSelect = new SelectStatement();
        _currentQuery = new QuerySpecification();
        _currentSelect.QueryExpression = _currentQuery;

        base.Visit(expression);

        return _errors.Count == 0 ? _currentSelect : null;
    }

    /// <summary>
    /// Gets the last error encountered during translation.
    /// </summary>
    public string? GetLastError() => _errors.LastOrDefault();

    /// <inheritdoc/>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.DeclaringType == typeof(Queryable))
        {
            return HandleQueryableMethod(node);
        }

        if (node.Method.DeclaringType == typeof(Enumerable))
        {
            return HandleEnumerableMethod(node);
        }

        _errors.Add($"Unsupported method: {node.Method.Name}");
        return node;
    }

    /// <inheritdoc/>
    protected override Expression VisitBinary(BinaryExpression node)
    {
        if (_currentQuery == null)
        {
            _errors.Add("No active query context");
            return node;
        }

        var comparison = CreateComparisonFromBinary(node);
        if (comparison != null)
        {
            if (_currentQuery.WhereClause == null)
            {
                _currentQuery.WhereClause = new WhereClause();
            }
            _currentQuery.WhereClause.SearchCondition = comparison;
        }

        return base.VisitBinary(node);
    }

    /// <inheritdoc/>
    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (node.Value is IQueryable queryable)
        {
            // This is likely the table source
            var tableName = queryable.ElementType.Name;
            var tableReference = new NamedTableReference
            {
                SchemaObject = new SchemaObjectName
                {
                    Identifiers =
                    {
                        new Identifier { Value = "dbo" },
                        new Identifier { Value = tableName }
                    }
                }
            };

            if (_currentQuery != null)
            {
                _currentQuery.FromClause = new FromClause();
                _currentQuery.FromClause.TableReferences.Add(tableReference);
            }
        }

        return base.VisitConstant(node);
    }

    private Expression HandleQueryableMethod(MethodCallExpression node)
    {
        switch (node.Method.Name)
        {
            case "Where":
                return HandleWhere(node);
            case "Select":
                return HandleSelect(node);
            case "OrderBy":
            case "OrderByDescending":
                return HandleOrderBy(node);
            case "Take":
                return HandleTake(node);
            case "Skip":
                return HandleSkip(node);
            default:
                _errors.Add($"Unsupported Queryable method: {node.Method.Name}");
                return node;
        }
    }

    private Expression HandleEnumerableMethod(MethodCallExpression node)
    {
        // Similar to Queryable but for in-memory operations
        return HandleQueryableMethod(node);
    }

    private Expression HandleWhere(MethodCallExpression node)
    {
        if (node.Arguments.Count < 2)
        {
            _errors.Add("Where requires a predicate");
            return node;
        }

        // Visit the source
        Visit(node.Arguments[0]);

        // Visit the predicate
        var predicate = node.Arguments[1];
        if (predicate is UnaryExpression unary && unary.Operand is LambdaExpression lambda)
        {
            Visit(lambda.Body);
        }

        return node;
    }

    private Expression HandleSelect(MethodCallExpression node)
    {
        if (node.Arguments.Count < 2)
        {
            _errors.Add("Select requires a selector");
            return node;
        }

        // Visit the source
        Visit(node.Arguments[0]);

        // Handle the selector
        var selector = node.Arguments[1];
        if (selector is UnaryExpression unary && unary.Operand is LambdaExpression lambda)
        {
            // Process the projection
            ProcessProjection(lambda);
        }

        return node;
    }

    private Expression HandleOrderBy(MethodCallExpression node)
    {
        if (_currentQuery == null || node.Arguments.Count < 2)
        {
            _errors.Add("Invalid OrderBy");
            return node;
        }

        // Visit the source
        Visit(node.Arguments[0]);

        // Handle the key selector
        var keySelector = node.Arguments[1];
        if (keySelector is UnaryExpression unary && unary.Operand is LambdaExpression lambda)
        {
            var orderByClause = new OrderByClause();
            var element = new ExpressionWithSortOrder
            {
                Expression = CreateColumnReference(lambda.Body),
                SortOrder = node.Method.Name.Contains("Descending")
                    ? SortOrder.Descending
                    : SortOrder.Ascending
            };
            orderByClause.OrderByElements.Add(element);
            _currentQuery.OrderByClause = orderByClause;
        }

        return node;
    }

    private Expression HandleTake(MethodCallExpression node)
    {
        if (_currentQuery == null || node.Arguments.Count < 2)
        {
            _errors.Add("Invalid Take");
            return node;
        }

        // Visit the source
        Visit(node.Arguments[0]);

        // Get the count
        if (node.Arguments[1] is ConstantExpression constant && constant.Value is int count)
        {
            _currentQuery.TopRowFilter = new TopRowFilter
            {
                Expression = new IntegerLiteral { Value = count.ToString() }
            };
        }

        return node;
    }

    private Expression HandleSkip(MethodCallExpression node)
    {
        if (_currentQuery == null || node.Arguments.Count < 2)
        {
            _errors.Add("Invalid Skip");
            return node;
        }

        // Visit the source
        Visit(node.Arguments[0]);

        // Get the count
        if (node.Arguments[1] is ConstantExpression constant && constant.Value is int count)
        {
            // SQL Server uses OFFSET for Skip
            _currentQuery.OffsetClause = new OffsetClause
            {
                OffsetExpression = new IntegerLiteral { Value = count.ToString() },
                FetchExpression = null
            };
        }

        return node;
    }

    private void ProcessProjection(LambdaExpression lambda)
    {
        if (_currentQuery == null) return;

        // Simple case: selecting all columns
        if (lambda.Body == lambda.Parameters[0])
        {
            _currentQuery.SelectElements.Add(new SelectStarExpression());
            return;
        }

        // Handle member initialization (anonymous types)
        if (lambda.Body is MemberInitExpression memberInit)
        {
            foreach (var binding in memberInit.Bindings)
            {
                if (binding is MemberAssignment assignment)
                {
                    var column = CreateColumnReference(assignment.Expression);
                    var element = new SelectScalarExpression
                    {
                        Expression = column,
                        ColumnName = new IdentifierOrValueExpression
                        {
                            Identifier = new Identifier { Value = binding.Member.Name }
                        }
                    };
                    _currentQuery.SelectElements.Add(element);
                }
            }
        }
        else
        {
            // Single column selection
            var column = CreateColumnReference(lambda.Body);
            var element = new SelectScalarExpression { Expression = column };
            _currentQuery.SelectElements.Add(element);
        }
    }

    private ScalarExpression CreateColumnReference(Expression expression)
    {
        if (expression is MemberExpression member)
        {
            return new ColumnReferenceExpression
            {
                MultiPartIdentifier = new MultiPartIdentifier
                {
                    Identifiers = { new Identifier { Value = member.Member.Name } }
                }
            };
        }

        // Default to a literal for now
        return new StringLiteral { Value = expression.ToString() };
    }

    private BooleanExpression? CreateComparisonFromBinary(BinaryExpression node)
    {
        var left = CreateColumnReference(node.Left);
        var right = CreateColumnReference(node.Right);

        BooleanComparisonType comparisonType = node.NodeType switch
        {
            ExpressionType.Equal => BooleanComparisonType.Equals,
            ExpressionType.NotEqual => BooleanComparisonType.NotEqualToBrackets,
            ExpressionType.GreaterThan => BooleanComparisonType.GreaterThan,
            ExpressionType.GreaterThanOrEqual => BooleanComparisonType.GreaterThanOrEqualTo,
            ExpressionType.LessThan => BooleanComparisonType.LessThan,
            ExpressionType.LessThanOrEqual => BooleanComparisonType.LessThanOrEqualTo,
            _ => BooleanComparisonType.Equals
        };

        return new BooleanComparisonExpression
        {
            ComparisonType = comparisonType,
            FirstExpression = left,
            SecondExpression = right
        };
    }
}