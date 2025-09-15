namespace TemplateNamespace.EntityName;

/// <summary>
/// Response model representing a EntityName record.
/// </summary>
public class EntityNameResponse
{
    /// <summary>
    /// The unique identifier for the EntityName.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the EntityName.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// When the EntityName was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the EntityName was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Whether the EntityName is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // TODO: Add additional properties specific to your EntityName entity
    // For example:
    // public string? Description { get; set; }
    // public string? Category { get; set; }
    // public decimal? Price { get; set; }
}