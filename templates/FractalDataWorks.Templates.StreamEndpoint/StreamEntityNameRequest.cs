using System.ComponentModel.DataAnnotations;
using FastEndpoints;

namespace TemplateNamespace.EntityName;

/// <summary>
/// Request model for streaming EntityName data.
/// </summary>
public class StreamEntityNameRequest
{
    /// <summary>
    /// Interval between streamed items in milliseconds.
    /// </summary>
    [Range(100, 60000, ErrorMessage = "Interval must be between 100ms and 60 seconds")]
    public int IntervalMs { get; set; } = 1000;

    /// <summary>
    /// Maximum number of items to stream (0 for unlimited).
    /// </summary>
    [Range(0, 10000, ErrorMessage = "MaxItems must be between 0 and 10000")]
    public int MaxItems { get; set; } = 100;

#if (EnableFilters)
    /// <summary>
    /// Optional filter for EntityName name search.
    /// </summary>
    [StringLength(100, ErrorMessage = "Search filter cannot exceed 100 characters")]
    public string? Search { get; set; }

    /// <summary>
    /// Optional category filter.
    /// </summary>
    [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
    public string? Category { get; set; }

    /// <summary>
    /// Optional date filter - only stream items created after this date.
    /// </summary>
    public DateTime? CreatedAfter { get; set; }

    /// <summary>
    /// Whether to include inactive EntityName records.
    /// </summary>
    public bool IncludeInactive { get; set; } = false;
#endif

    // TODO: Add additional filtering properties specific to your EntityName entity
    // For example:
    // public string? Status { get; set; }
    // public decimal? MinPrice { get; set; }
    // public decimal? MaxPrice { get; set; }
    // public string? AssignedToUser { get; set; }
}

/// <summary>
/// Validator for StreamEntityNameRequest.
/// </summary>
public class StreamEntityNameRequestValidator : Validator<StreamEntityNameRequest>
{
    public StreamEntityNameRequestValidator()
    {
        RuleFor(x => x.IntervalMs)
            .InclusiveBetween(100, 60000)
            .WithMessage("Interval must be between 100ms and 60 seconds");

        RuleFor(x => x.MaxItems)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(10000)
            .WithMessage("MaxItems must be between 0 and 10000");

#if (EnableFilters)
        RuleFor(x => x.Search)
            .MaximumLength(100)
            .WithMessage("Search filter cannot exceed 100 characters");

        RuleFor(x => x.Category)
            .MaximumLength(50)
            .WithMessage("Category cannot exceed 50 characters");

        RuleFor(x => x.CreatedAfter)
            .LessThan(DateTime.UtcNow)
            .When(x => x.CreatedAfter.HasValue)
            .WithMessage("CreatedAfter must be in the past");
#endif

        // TODO: Add custom validation rules specific to your EntityName streaming requirements
        // For example:
        // RuleFor(x => x.MaxItems)
        //     .LessThanOrEqualTo(1000)
        //     .When(x => x.IntervalMs < 500)
        //     .WithMessage("Cannot stream more than 1000 items with interval less than 500ms");
    }
}