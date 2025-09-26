using FractalDataWorks.Collections;

namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Interface for logical operators.
/// </summary>
public interface ILogicalOperator : ITypeOption<ILogicalOperator>
{
    /// <summary>
    /// Gets the description of this logical operator.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the SQL operator string for this logical operator.
    /// </summary>
    string SqlOperator { get; }

    /// <summary>
    /// Gets the precedence level of this logical operator for expression evaluation.
    /// </summary>
    int Precedence { get; }
}