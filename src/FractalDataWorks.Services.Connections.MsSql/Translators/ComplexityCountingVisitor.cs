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