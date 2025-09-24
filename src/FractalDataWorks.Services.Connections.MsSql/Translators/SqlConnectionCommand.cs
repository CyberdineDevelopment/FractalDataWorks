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
/// Connection command wrapper for SQL Server commands.
/// </summary>
internal sealed class SqlConnectionCommand : IConnectionCommand
{
    public SqlConnectionCommand(SqlCommand sqlCommand)
    {
        SqlCommand = sqlCommand ?? throw new ArgumentNullException(nameof(sqlCommand));
    }

    public SqlCommand SqlCommand { get; }
}