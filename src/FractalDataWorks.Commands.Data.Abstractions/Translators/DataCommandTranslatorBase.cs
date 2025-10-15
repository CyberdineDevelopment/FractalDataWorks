namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Abstract base class for data command translators.
/// This base class is used by TypeCollection source generators for compile-time discovery.
/// </summary>
/// <remarks>
/// <para>
/// This base class is primarily for compile-time translator discovery via [TypeOption].
/// Most translators will be registered at runtime by connections via DataCommandTranslators.Register().
/// </para>
/// <para>
/// Properties must be set in constructor for TypeCollection source generators to read them.
/// </para>
/// </remarks>
public abstract class DataCommandTranslatorBase : IDataCommandTranslator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandTranslatorBase"/> class.
    /// </summary>
    /// <param name="id">Unique identifier for this translator type.</param>
    /// <param name="name">Name of the translator (must match TypeOption attribute if used).</param>
    /// <param name="domainName">Domain name this translator targets.</param>
    protected DataCommandTranslatorBase(int id, string name, string domainName)
    {
        Id = id;
        Name = name;
        DomainName = domainName;
    }

    /// <summary>
    /// Gets the unique identifier for this translator type.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of this translator type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the domain name this translator targets.
    /// </summary>
    public string DomainName { get; }

    /// <summary>
    /// Translates a data command to a connection-specific command.
    /// </summary>
    public abstract System.Threading.Tasks.Task<FractalDataWorks.Results.IGenericResult<FractalDataWorks.Services.Connections.Abstractions.IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        System.Threading.CancellationToken cancellationToken = default);
}
