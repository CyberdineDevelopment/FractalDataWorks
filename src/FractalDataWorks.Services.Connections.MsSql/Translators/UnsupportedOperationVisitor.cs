using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using FractalDataWorks.Data.DataSets.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions.Translators;

namespace FractalDataWorks.Services.Connections.MsSql.Translators;

/// <summary>
/// Visitor that detects unsupported LINQ operations for T-SQL translation.
/// </summary>
internal sealed class UnsupportedOperationVisitor : ExpressionVisitor
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