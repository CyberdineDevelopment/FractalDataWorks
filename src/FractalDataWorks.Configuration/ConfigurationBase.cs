using System;
using System.Collections.Generic;
using System.Linq;

using FractalDataWorks.Results;
using FluentValidation;
using FluentValidation.Results;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Base class for all configuration types in the Rec framework.
/// </summary>
/// <typeparam name="TConfiguration">The derived configuration type.</typeparam>
public abstract class ConfigurationBase<TConfiguration> : FractalConfigurationBase
    where TConfiguration : ConfigurationBase<TConfiguration>, new()
{

    /// <summary>
    /// Gets the timestamp when this configuration was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when this configuration was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Gets the section name for this configuration.
    /// </summary>
    public override abstract string SectionName { get; }

    /// <summary>
    /// Validates the configuration using FluentValidation.
    /// </summary>
    /// <returns>A FdwResult containing the FluentValidation ValidationResult.</returns>
    public override IFdwResult<ValidationResult> Validate()
    {
        var validator = GetValidator();
        if (validator == null)
        {
            // No validator configured, return success
            return FdwResult<ValidationResult>.Success(new ValidationResult());
        }

        var validationResult = validator.Validate((TConfiguration)this);
        // Always return success with the validation result
        // The caller should check validationResult.IsValid to determine if validation passed
        return FdwResult<ValidationResult>.Success(validationResult);
    }



    /// <summary>
    /// Gets the validator for this configuration type.
    /// </summary>
    /// <returns>The validator instance or null if no validation is required.</returns>
    protected virtual IValidator<TConfiguration>? GetValidator()
    {
        return null;
    }

    /// <summary>
    /// Marks this configuration as modified.
    /// </summary>
    protected void MarkAsModified()
    {
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a clone of this configuration.
    /// </summary>
    /// <returns>A cloned instance of the configuration.</returns>
    public virtual TConfiguration Clone()
    {
        var clone = new TConfiguration();
        CopyTo(clone);
        return clone;
    }

    /// <summary>
    /// Copies the properties of this configuration to another instance.
    /// </summary>
    /// <param name="target">The target configuration.</param>
    protected virtual void CopyTo(TConfiguration target)
    {
        target.Id = Id;
        target.Name = Name;
        target.IsEnabled = IsEnabled;
        target.CreatedAt = CreatedAt;
        target.ModifiedAt = ModifiedAt;
    }
}
