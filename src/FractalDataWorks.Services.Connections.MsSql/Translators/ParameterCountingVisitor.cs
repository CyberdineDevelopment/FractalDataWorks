using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using FractalDataWorks.DataSets.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions.Translators;

namespace FractalDataWorks.Services.Connections.MsSql.Translators;

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