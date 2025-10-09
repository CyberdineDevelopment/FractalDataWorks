using System.Collections.Generic;
using System.IO;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using FractalDataWorks.Data.Sql.Messages;
using FractalDataWorks.Results;

namespace FractalDataWorks.Data.Sql.Translators;

/// <summary>
/// Generates SQL text from T-SQL fragments using ScriptDom.
/// </summary>
public sealed class TSqlGenerator
{
    /// <summary>
    /// Result of SQL generation.
    /// </summary>
    public sealed record SqlGenerationResult(string Sql, IReadOnlyDictionary<string, object> Parameters);

    /// <summary>
    /// Generates SQL text from a T-SQL fragment.
    /// </summary>
    public IGenericResult<SqlGenerationResult> GenerateSql(TSqlFragment fragment)
    {
        try
        {
            var generator = new Sql170ScriptGenerator();
            var writer = new StringWriter();

            generator.GenerateScript(fragment, writer);
            var sql = writer.ToString();

            // Extract parameters (simplified for now)
            var parameters = ExtractParameters(fragment);

            return GenericResult.Success(new SqlGenerationResult(sql, parameters));
        }
        catch (System.Exception ex)
        {
            return GenericResult.Failure<SqlGenerationResult>(
                SqlMessages.SyntaxError(ex.Message));
        }
    }

    private IReadOnlyDictionary<string, object> ExtractParameters(TSqlFragment fragment)
    {
        var parameters = new Dictionary<string, object>();
        var visitor = new ParameterExtractorVisitor(parameters);
        fragment.Accept(visitor);
        return parameters;
    }

    /// <summary>
    /// Visitor to extract parameters from T-SQL fragments.
    /// </summary>
    private sealed class ParameterExtractorVisitor : TSqlFragmentVisitor
    {
        private readonly Dictionary<string, object> _parameters;
        private int _parameterIndex;

        public ParameterExtractorVisitor(Dictionary<string, object> parameters)
        {
            _parameters = parameters;
        }

        public override void Visit(VariableReference node)
        {
            if (!_parameters.ContainsKey(node.Name))
            {
                _parameters[node.Name] = null; // Placeholder
            }
            base.Visit(node);
        }

        public override void Visit(Literal node)
        {
            // Convert literals to parameters for safety
            var paramName = $"@p{_parameterIndex++}";
            _parameters[paramName] = node.Value;
            base.Visit(node);
        }
    }
}