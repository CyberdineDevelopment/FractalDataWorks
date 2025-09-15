using System.ComponentModel.DataAnnotations;

namespace TemplateNamespace.EntityName;

/// <summary>
/// Request model for CommandAction EntityName operation.
/// </summary>
public class CommandActionEntityNameRequest
{
#if (Action == "Update" || Action == "Delete")
    /// <summary>
    /// The unique identifier of the EntityName to CommandAction.
    /// </summary>
    [Required]
    public int Id { get; set; }
#endif

#if (Action == "Create" || Action == "Update")
    /// <summary>
    /// The name of the EntityName.
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description for the EntityName.
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Whether the EntityName should be active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // TODO: Add additional properties specific to your EntityName entity
    // For example:
    // [Range(0, double.MaxValue, ErrorMessage = "Price must be positive")]
    // public decimal? Price { get; set; }
    // 
    // [EmailAddress(ErrorMessage = "Invalid email format")]
    // public string? Email { get; set; }
    // 
    // public DateTime? StartDate { get; set; }
#endif
}

/// <summary>
/// Validator for CommandActionEntityNameRequest.
/// </summary>
public class CommandActionEntityNameRequestValidator : Validator<CommandActionEntityNameRequest>
{
    public CommandActionEntityNameRequestValidator()
    {
#if (Action == "Update" || Action == "Delete")
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Valid EntityName ID is required");
#endif

#if (Action == "Create" || Action == "Update")
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .Length(2, 100)
            .WithMessage("Name must be between 2 and 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        // TODO: Add custom validation rules specific to your EntityName
        // For example:
        // RuleFor(x => x.Email)
        //     .EmailAddress()
        //     .When(x => !string.IsNullOrEmpty(x.Email))
        //     .WithMessage("Please provide a valid email address");
        //
        // RuleFor(x => x.Price)
        //     .GreaterThan(0)
        //     .When(x => x.Price.HasValue)
        //     .WithMessage("Price must be greater than 0");
#endif
    }
}