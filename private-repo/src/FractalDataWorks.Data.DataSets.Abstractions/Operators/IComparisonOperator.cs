using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.DataSets.Abstractions.Operators;

/// <summary>
/// Interface for comparison operators.
/// </summary>
public interface IComparisonOperator : ITypeOption<ComparisonOperatorBase>
{
    /// <summary>
    /// Gets the description of this comparison operator.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the SQL operator string for this comparison operator.
    /// </summary>
    string SqlOperator { get; }

    /// <summary>
    /// Gets a value indicating whether this operator accepts a single value or multiple values.
    /// </summary>
    bool IsSingleValue { get; }
}