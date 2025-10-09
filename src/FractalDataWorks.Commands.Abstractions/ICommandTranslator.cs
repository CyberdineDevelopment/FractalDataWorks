using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Data.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Commands.Abstractions;

/// <summary>
/// Interface for command translators that convert between different command formats.
/// </summary>
/// <remarks>
/// Translators are responsible for converting commands from one format to another,
/// such as translating LINQ expressions to SQL queries or HTTP requests.
/// All translators must return IGenericResult with appropriate messages on failure.
/// </remarks>
public interface ICommandTranslator
{
    /// <summary>
    /// Gets the translator type metadata.
    /// </summary>
    ITranslatorType TranslatorType { get; }

    /// <summary>
    /// Validates whether this translator can handle the given expression.
    /// </summary>
    /// <param name="expression">The expression to validate.</param>
    /// <returns>A result indicating whether the expression can be translated.</returns>
    IGenericResult<bool> CanTranslate(Expression expression);

    /// <summary>
    /// Translates an expression into a command.
    /// </summary>
    /// <param name="expression">The expression to translate.</param>
    /// <param name="context">Optional translation context with metadata.</param>
    /// <returns>A result containing the translated command or failure message.</returns>
    IGenericResult<ICommand> Translate(Expression expression, ITranslationContext? context = null);

    /// <summary>
    /// Translates a command from one format to another.
    /// </summary>
    /// <param name="command">The command to translate.</param>
    /// <param name="targetFormat">The desired output format.</param>
    /// <returns>A result containing the translated command or failure message.</returns>
    IGenericResult<ICommand> TranslateCommand(ICommand command, IDataFormat targetFormat);

    /// <summary>
    /// Optimizes a translated command for better performance.
    /// </summary>
    /// <param name="command">The command to optimize.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A result containing the optimized command or the original if optimization isn't possible.</returns>
    Task<IGenericResult<ICommand>> Optimize(ICommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates the cost of executing a translated command.
    /// </summary>
    /// <param name="command">The command to analyze.</param>
    /// <returns>A result containing cost estimation metrics.</returns>
    IGenericResult<CommandCostEstimate> EstimateCost(ICommand command);
}