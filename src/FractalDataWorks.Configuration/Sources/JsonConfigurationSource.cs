using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Configuration.Abstractions;

namespace FractalDataWorks.Configuration.Sources;

/// <summary>
/// Configuration source that reads and writes JSON files.
/// </summary>
public class JsonConfigurationSource : ConfigurationSourceBase
{
    private readonly string _basePath;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonConfigurationSource"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="basePath">The base path for JSON configuration files.</param>
    public JsonConfigurationSource(ILogger<JsonConfigurationSource> logger, string basePath)
        : base(logger, "JSON")
    {
        Logger = logger;
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));

        // Ensure the directory exists
        Directory.CreateDirectory(_basePath);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <inheritdoc/>
    public override bool IsWritable => true;

    /// <inheritdoc/>
    public override bool SupportsReload => false;

    /// <inheritdoc/>
    public override Task<IGenericResult<IEnumerable<TConfiguration>>> Load<TConfiguration>()
    {
        var typeName = typeof(TConfiguration).Name;
        var pattern = $"{typeName}_*.json";
        var files = Directory.GetFiles(_basePath, pattern);

        var configurations = new List<TConfiguration>();

        foreach (var file in files)
        {
            try
            {
                var json = File.ReadAllText(file);
                var config = JsonSerializer.Deserialize<TConfiguration>(json, _jsonOptions);

                if (config != null)
                {
                    configurations.Add(config);
                }
            }
            catch (Exception ex)
            {
                // Log but continue loading other files
                ConfigurationSourceBaseLog.LoadFailed(Logger, Name, ex.Message);
            }
        }

        return Task.FromResult(GenericResult<IEnumerable<TConfiguration>>.Success(configurations));
    }

    /// <summary>
    /// Loads a configuration by ID from this source.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to load.</typeparam>
    /// <param name="id">The ID of the configuration to load.</param>
    /// <returns>A task containing the loaded configuration.</returns>
    public Task<IGenericResult<TConfiguration>> Load<TConfiguration>(int id)
        where TConfiguration : IGenericConfiguration
    {
        var fileName = GetFileName<TConfiguration>(id);
        var filePath = Path.Combine(_basePath, fileName);

        if (!File.Exists(filePath))
        {
            return Task.FromResult(GenericResult<TConfiguration>.Failure($"Configuration file not found: {fileName}"));
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<TConfiguration>(json, _jsonOptions);

            if (config == null)
            {
                return Task.FromResult(GenericResult<TConfiguration>.Failure("Failed to deserialize configuration"));
            }

            return Task.FromResult(GenericResult<TConfiguration>.Success(config));
        }
        catch (Exception ex)
        {
            return Task.FromResult(GenericResult<TConfiguration>.Failure($"Error loading configuration: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public override Task<IGenericResult<TConfiguration>> Save<TConfiguration>(TConfiguration configuration)
    {
        if (!IsWritable)
        {
            return Task.FromResult<IGenericResult<TConfiguration>>(
                GenericResult<TConfiguration>.Failure<TConfiguration>($"Configuration source '{Name}' is read-only"));
        }

        var fileName = GetFileName(configuration);
        var filePath = Path.Combine(_basePath, fileName);

        try
        {
            var json = JsonSerializer.Serialize(configuration, _jsonOptions);
            File.WriteAllText(filePath, json);

            // Try to get ID if configuration has it
            int configId = 0;
            if (configuration is object obj && obj.GetType().GetProperty("Id") is { } idProperty)
            {
                configId = idProperty.GetValue(obj) as int? ?? 0;
            }
            ConfigurationSourceBaseLog.ConfigurationSaved(Logger, Name, configId);

            return Task.FromResult(GenericResult<TConfiguration>.Success(configuration));
        }
        catch (Exception ex)
        {
            return Task.FromResult(GenericResult<TConfiguration>.Failure($"Error saving configuration: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public override async Task<IGenericResult<NonResult>> Delete<TConfiguration>(int id)
    {
        if (!IsWritable)
        {
            return GenericResult<NonResult>.Failure<NonResult>($"Configuration source '{Name}' is read-only");
        }

        var fileName = GetFileName<TConfiguration>(id);
        var filePath = Path.Combine(_basePath, fileName);

        if (!File.Exists(filePath))
        {
            return GenericResult<NonResult>.Failure($"Configuration file not found: {fileName}");
        }

        try
        {
            await Task.Run(() => File.Delete(filePath)).ConfigureAwait(false);

            ConfigurationSourceBaseLog.ConfigurationDeleted(Logger, Name, id);

            return GenericResult<NonResult>.Success(NonResult.Value);
        }
        catch (Exception ex)
        {
            return GenericResult<NonResult>.Failure($"Error deleting configuration: {ex.Message}");
        }
    }

    private static string GetFileName<TConfiguration>(TConfiguration configuration)
        where TConfiguration : IGenericConfiguration
    {
        // Try to get ID if configuration has it
        int configId = 0;
        if (configuration is object obj && obj.GetType().GetProperty("Id") is { } idProperty)
        {
            configId = idProperty.GetValue(obj) as int? ?? 0;
        }
        return GetFileName<TConfiguration>(configId);
    }

    private static string GetFileName<TConfiguration>(int id)
        where TConfiguration : IGenericConfiguration
    {
        var typeName = typeof(TConfiguration).Name;
        return $"{typeName}_{id}.json";
    }
}
