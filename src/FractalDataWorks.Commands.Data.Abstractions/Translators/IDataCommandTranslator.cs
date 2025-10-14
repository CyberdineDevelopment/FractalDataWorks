using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Commands.Abstractions;
using FractalDataWorks.Data.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Interface for data command translators.
/// Translators convert universal IDataCommand to domain-specific IConnectionCommand.
/// </summary>
/// <remarks>
/// <para>
/// Translators bridge the gap between universal data commands and connection-specific commands:
/// <list type="bullet">
/// <item>SQL Translator: IDataCommand → SQL string with parameters</item>
/// <item>REST Translator: IDataCommand → HTTP request with OData query</item>
/// <item>File Translator: IDataCommand → File operations</item>
/// <item>GraphQL Translator: IDataCommand → GraphQL query</item>
/// </list>
/// </para>
/// <para>
/// Translators are registered:
/// <list type="bullet">
/// <item>Compile-time: Via [TypeOption] attribute (discovered by source generator)</item>
/// <item>Runtime: Via DataCommandTranslators.Register() (for connection-provided translators)</item>
/// </list>
/// </para>
/// </remarks>
public interface IDataCommandTranslator
{
    /// <summary>
    /// Gets the domain name this translator targets (Sql, Rest, File, GraphQL, etc.).
    /// </summary>
    string DomainName { get; }

    /// <summary>
    /// Translates a data command to a connection-specific command.
    /// </summary>
    /// <param name="command">The data command to translate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the translated connection command.</returns>
    Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        CancellationToken cancellationToken = default);
}
