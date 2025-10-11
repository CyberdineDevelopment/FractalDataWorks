using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FractalDataWorks.Data.Sql.Translators;

/// <summary>
/// Visits LINQ expressions and converts them to T-SQL fragments.
/// </summary>
public sealed class LinqToTSqlVisitor : ExpressionVisitor
{
    private readonly Stack<Microsoft.SqlServer.TransactSql.ScriptDom.TSqlFragment> _fragments = new();
    private readonly List<string> _errors = new();
    private Microsoft.SqlServer.TransactSql.ScriptDom.SelectStatement? _currentSelect;
    private Microsoft.SqlServer.TransactSql.ScriptDom.QuerySpecification? _currentQuery;

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
    public new Microsoft.SqlServer.TransactSql.ScriptDom.TSqlFragment? Visit(Expression expression)
    {
        _errors.Clear();
        _fragments.Clear();
        _currentSelect = new Microsoft.SqlServer.TransactSql.ScriptDom.SelectStatement();
        _currentQuery = new Microsoft.SqlServer.TransactSql.ScriptDom.QuerySpecification();
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
                _currentQuery.WhereClause = new Microsoft.SqlServer.TransactSql.ScriptDom.WhereClause();
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
            var tableReference = new Microsoft.SqlServer.TransactSql.ScriptDom.NamedTableReference
            {
                SchemaObject = new Microsoft.SqlServer.TransactSql.ScriptDom.SchemaObjectName
                {
                    Identifiers =
                    {
                        new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier { Value = "dbo" },
                        new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier { Value = tableName }
                    }
                }
            };

            if (_currentQuery != null)
            {
                _currentQuery.FromClause = new Microsoft.SqlServer.TransactSql.ScriptDom.FromClause();
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
            var orderByClause = new Microsoft.SqlServer.TransactSql.ScriptDom.OrderByClause();
            var element = new Microsoft.SqlServer.TransactSql.ScriptDom.ExpressionWithSortOrder
            {
                Expression = CreateColumnReference(lambda.Body),
                SortOrder = node.Method.Name.Contains("Descending")
                    ? Microsoft.SqlServer.TransactSql.ScriptDom.SortOrder.Descending
                    : Microsoft.SqlServer.TransactSql.ScriptDom.SortOrder.Ascending
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
            _currentQuery.TopRowFilter = new Microsoft.SqlServer.TransactSql.ScriptDom.TopRowFilter
            {
                Expression = new Microsoft.SqlServer.TransactSql.ScriptDom.IntegerLiteral { Value = count.ToString() }
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
            _currentQuery.OffsetClause = new Microsoft.SqlServer.TransactSql.ScriptDom.OffsetClause
            {
                OffsetExpression = new Microsoft.SqlServer.TransactSql.ScriptDom.IntegerLiteral { Value = count.ToString() },
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
            _currentQuery.SelectElements.Add(new Microsoft.SqlServer.TransactSql.ScriptDom.SelectStarExpression());
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
                    var element = new Microsoft.SqlServer.TransactSql.ScriptDom.SelectScalarExpression
                    {
                        Expression = column,
                        ColumnName = new Microsoft.SqlServer.TransactSql.ScriptDom.IdentifierOrValueExpression
                        {
                            Identifier = new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier { Value = binding.Member.Name }
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
            var element = new Microsoft.SqlServer.TransactSql.ScriptDom.SelectScalarExpression { Expression = column };
            _currentQuery.SelectElements.Add(element);
        }
    }

    private Microsoft.SqlServer.TransactSql.ScriptDom.ScalarExpression CreateColumnReference(Expression expression)
    {
        if (expression is MemberExpression member)
        {
            return new Microsoft.SqlServer.TransactSql.ScriptDom.ColumnReferenceExpression
            {
                MultiPartIdentifier = new Microsoft.SqlServer.TransactSql.ScriptDom.MultiPartIdentifier
                {
                    Identifiers = { new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier { Value = member.Member.Name } }
                }
            };
        }

        // Default to a literal for now
        return new Microsoft.SqlServer.TransactSql.ScriptDom.StringLiteral { Value = expression.ToString() };
    }

    private Microsoft.SqlServer.TransactSql.ScriptDom.BooleanExpression? CreateComparisonFromBinary(BinaryExpression node)
    {
        var left = CreateColumnReference(node.Left);
        var right = CreateColumnReference(node.Right);

        Microsoft.SqlServer.TransactSql.ScriptDom.BooleanComparisonType comparisonType = node.NodeType switch
        {
            ExpressionType.Equal => Microsoft.SqlServer.TransactSql.ScriptDom.BooleanComparisonType.Equals,
            ExpressionType.NotEqual => Microsoft.SqlServer.TransactSql.ScriptDom.BooleanComparisonType.NotEqualToBrackets,
            ExpressionType.GreaterThan => Microsoft.SqlServer.TransactSql.ScriptDom.BooleanComparisonType.GreaterThan,
            ExpressionType.GreaterThanOrEqual => Microsoft.SqlServer.TransactSql.ScriptDom.BooleanComparisonType.GreaterThanOrEqualTo,
            ExpressionType.LessThan => Microsoft.SqlServer.TransactSql.ScriptDom.BooleanComparisonType.LessThan,
            ExpressionType.LessThanOrEqual => Microsoft.SqlServer.TransactSql.ScriptDom.BooleanComparisonType.LessThanOrEqualTo,
            _ => Microsoft.SqlServer.TransactSql.ScriptDom.BooleanComparisonType.Equals
        };

        return new Microsoft.SqlServer.TransactSql.ScriptDom.BooleanComparisonExpression
        {
            ComparisonType = comparisonType,
            FirstExpression = left,
            SecondExpression = right
        };
    }
}