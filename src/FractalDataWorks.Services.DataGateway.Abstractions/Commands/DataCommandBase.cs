using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.DataGateway.Abstractions.Models;
using FluentValidation.Results;
using FractalDataWorks.Configuration.Abstractions;


namespace FractalDataWorks.Services.DataGateway.Abstractions.Commands;

/// <summary>
/// Base class for all data commands providing provider-agnostic data operations.
/// </summary>
/// <remarks>
/// DataCommandBase represents universal data operations that can be translated to provider-specific
/// implementations. This enables the same LINQ expressions and commands to work across SQL databases,
/// file systems, REST APIs, and other data sources.
/// </remarks>
public abstract class DataCommandBase : IDataCommand
{
    private readonly Dictionary<string, object?> _parameters;
    private readonly Dictionary<string, object> _metadata;

    /// <inheritdoc/>
    public Guid CommandId { get; }

    /// <inheritdoc/>
    public Guid CorrelationId { get; }

    /// <inheritdoc/>
    public DateTimeOffset Timestamp { get; }

    /// <inheritdoc/>
    public IFractalConfiguration? Configuration { get; }

    /// <inheritdoc/>
    public string CommandType { get; private set; }

    /// <inheritdoc/>
    public string? Target => TargetContainer?.ToString();

    /// <inheritdoc/>
    public Type ExpectedResultType { get; private set; }

    /// <inheritdoc/>
    public TimeSpan? Timeout { get; set; }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object?> Parameters => _parameters;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object> Metadata => _metadata;

    /// <summary>
    /// Gets the connection name for this command.
    /// </summary>
    public string? ConnectionName { get; private set; }

    /// <summary>
    /// Gets the target container path for this command.
    /// </summary>
    public DataPath? TargetContainer { get; private set; }

    /// <inheritdoc/>
    public abstract bool IsDataModifying { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandBase"/> class.
    /// </summary>
    /// <param name="commandType">The type of command (Query, Insert, Update, Delete, Upsert).</param>
    /// <param name="connectionName">The named connection to execute this command against.</param>
    /// <param name="targetContainer">The target data container path.</param>
    /// <param name="expectedResultType">The expected result type.</param>
    /// <param name="parameters">Command parameters.</param>
    /// <param name="metadata">Additional command metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or empty.</exception>
    protected DataCommandBase(
        string commandType,
        string? connectionName,
        DataPath? targetContainer,
        Type expectedResultType,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
    {
        if (string.IsNullOrWhiteSpace(commandType))
            throw new ArgumentException("Command type cannot be null or empty.", nameof(commandType));
        
        // Allow empty/null connection name during command construction - validation happens at execution time
        
        // Initialize ICommand properties
        CommandId = Guid.NewGuid();
        CorrelationId = Guid.NewGuid(); 
        Timestamp = DateTimeOffset.UtcNow;
        Configuration = null;
        
        // Initialize IDataCommand properties
        CommandType = commandType;
        ConnectionName = connectionName;
        TargetContainer = targetContainer;
        ExpectedResultType = expectedResultType;
        Timeout = timeout;
        
        // Initialize collections
        _parameters = parameters?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal) 
                     ?? new Dictionary<string, object?>(StringComparer.Ordinal);
        _metadata = metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal) 
                   ?? new Dictionary<string, object>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Sets the timeout for this command using fluent syntax.
    /// </summary>
    /// <param name="timeout">The timeout to use.</param>
    /// <returns>This command instance for fluent chaining.</returns>
    public DataCommandBase WithTimeout(TimeSpan timeout)
    {
        Timeout = timeout;
        return this;
    }

    /// <inheritdoc/>
    public virtual IFdwResult<ValidationResult> Validate()
    {
        var errors = new List<ValidationFailure>();

        if (string.IsNullOrWhiteSpace(CommandType))
        {
            errors.Add(new ValidationFailure(nameof(CommandType), "Command type cannot be null or empty."));
        }

        if (string.IsNullOrWhiteSpace(ConnectionName))
        {
            errors.Add(new ValidationFailure(nameof(ConnectionName), "Connection name cannot be null or empty."));
        }

        if (ExpectedResultType == null)
        {
            errors.Add(new ValidationFailure(nameof(ExpectedResultType), "Expected result type cannot be null."));
        }

        if (Timeout.HasValue && Timeout.Value <= TimeSpan.Zero)
        {
            errors.Add(new ValidationFailure(nameof(Timeout), "Timeout must be positive if specified."));
        }

        // Validate parameters don't contain null keys
        foreach (var parameter in Parameters)
        {
            if (string.IsNullOrWhiteSpace(parameter.Key))
            {
                errors.Add(new ValidationFailure(nameof(Parameters), "Parameter keys cannot be null or empty."));
                break;
            }
        }

        var validationResult = new ValidationResult(errors);
        if (validationResult.IsValid)
        {
            return FdwResult<ValidationResult>.Success(validationResult);
        }
        
        return FdwResult<ValidationResult>.Success(validationResult);
    }
    
    /// <inheritdoc/>
    public virtual IDataCommand WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        return CreateCopy(ConnectionName, TargetContainer, newParameters, Metadata, Timeout);
    }

    /// <inheritdoc/>
    public virtual IDataCommand WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        return CreateCopy(ConnectionName, TargetContainer, Parameters, newMetadata, Timeout);
    }

    /// <summary>
    /// Creates a new command with the specified connection name.
    /// </summary>
    /// <param name="connectionName">The connection name to use.</param>
    /// <returns>A new command instance with the specified connection name.</returns>
    public virtual DataCommandBase WithConnection(string? connectionName)
    {
        return CreateCopy(connectionName, TargetContainer, Parameters, Metadata, Timeout);
    }

    /// <summary>
    /// Creates a new command with the specified target container.
    /// </summary>
    /// <param name="targetContainer">The target container path.</param>
    /// <returns>A new command instance with the specified target container.</returns>
    public virtual DataCommandBase WithTarget(DataPath targetContainer)
    {
        return CreateCopy(ConnectionName, targetContainer, Parameters, Metadata, Timeout);
    }

    /// <summary>
    /// Creates a copy of this command with new values.
    /// </summary>
    /// <param name="connectionName">The connection name.</param>
    /// <param name="targetContainer">The target container.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="metadata">The metadata.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>A new command instance.</returns>
    protected abstract DataCommandBase CreateCopy(
        string? connectionName,
        DataPath? targetContainer,
        IReadOnlyDictionary<string, object?> parameters,
        IReadOnlyDictionary<string, object> metadata,
        TimeSpan? timeout);

    /// <summary>
    /// Gets a parameter value by name.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="name">The parameter name.</param>
    /// <returns>The parameter value converted to the specified type.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the parameter is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to the specified type.</exception>
    protected T? GetParameter<T>(string name)
    {
        if (!_parameters.TryGetValue(name, out var value))
            throw new KeyNotFoundException($"Parameter '{name}' not found.");
        
        if (value == null)
            return default(T);
        
        if (value is T directValue)
            return directValue;
        
        try
        {
            return (T)Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            throw new InvalidCastException($"Cannot convert parameter '{name}' value from {value.GetType().Name} to {typeof(T).Name}.", ex);
        }
    }

    /// <summary>
    /// Tries to get a parameter value by name.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The parameter value if found and converted successfully.</param>
    /// <returns>True if the parameter was found and converted successfully; otherwise, false.</returns>
    protected bool TryGetParameter<T>(string name, out T? value)
    {
        try
        {
            value = GetParameter<T>(name);
            return true;
        }
        catch
        {
            value = default(T);
            return false;
        }
    }

    /// <summary>
    /// Gets a metadata value by name.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="name">The metadata name.</param>
    /// <returns>The metadata value converted to the specified type.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the metadata is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to the specified type.</exception>
    protected T GetMetadata<T>(string name)
    {
        if (!_metadata.TryGetValue(name, out var value))
            throw new KeyNotFoundException($"Metadata '{name}' not found.");
        
        if (value is T directValue)
            return directValue;
        
        try
        {
            return (T)Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            throw new InvalidCastException($"Cannot convert metadata '{name}' value from {value?.GetType().Name ?? "null"} to {typeof(T).Name}.", ex);
        }
    }

    /// <summary>
    /// Tries to get a metadata value by name.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="name">The metadata name.</param>
    /// <param name="value">The metadata value if found and converted successfully.</param>
    /// <returns>True if the metadata was found and converted successfully; otherwise, false.</returns>
    protected bool TryGetMetadata<T>(string name, out T? value)
    {
        try
        {
            value = GetMetadata<T>(name);
            return true;
        }
        catch
        {
            value = default(T);
            return false;
        }
    }

    /// <summary>
    /// Returns a string representation of the command.
    /// </summary>
    /// <returns>A string describing the command.</returns>
    public override string ToString()
    {
        var target = TargetContainer != null ? $" -> {TargetContainer}" : "";
        return $"{CommandType}({ConnectionName}){target}";
    }
}

/// <summary>
/// Generic base class for data commands with typed result expectations.
/// </summary>
/// <typeparam name="TResult">The type of result expected from command execution.</typeparam>
public abstract class DataCommandBase<TResult> : DataCommandBase, IDataCommand<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandBase{TResult}"/> class.
    /// </summary>
    /// <param name="commandType">The type of command.</param>
    /// <param name="connectionName">The named connection.</param>
    /// <param name="targetContainer">The target container.</param>
    /// <param name="parameters">Command parameters.</param>
    /// <param name="metadata">Command metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    protected DataCommandBase(
        string commandType,
        string? connectionName,
        DataPath? targetContainer = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base(commandType, connectionName, targetContainer, typeof(TResult), parameters, metadata, timeout)
    {
    }

    /// <inheritdoc/>
    IDataCommand<TResult> IDataCommand<TResult>.WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        ArgumentNullException.ThrowIfNull(newParameters);
        return (IDataCommand<TResult>)CreateCopy(ConnectionName, TargetContainer, newParameters, Metadata, Timeout);
    }

    /// <inheritdoc/>
    IDataCommand<TResult> IDataCommand<TResult>.WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        ArgumentNullException.ThrowIfNull(newMetadata);
        return (IDataCommand<TResult>)CreateCopy(ConnectionName, TargetContainer, Parameters, newMetadata, Timeout);
    }

}
