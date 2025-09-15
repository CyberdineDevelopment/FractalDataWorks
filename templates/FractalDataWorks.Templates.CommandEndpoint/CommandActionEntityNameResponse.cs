namespace TemplateNamespace.EntityName;

/// <summary>
/// Response model for CommandAction EntityName operation.
/// </summary>
public class CommandActionEntityNameResponse
{
    /// <summary>
    /// The unique identifier of the EntityName.
    /// </summary>
    public int Id { get; set; }

#if (Action == "Create" || Action == "Update")
    /// <summary>
    /// The name of the EntityName.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The description of the EntityName.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the EntityName is active.
    /// </summary>
    public bool IsActive { get; set; }
#endif

#if (Action == "Create")
    /// <summary>
    /// When the EntityName was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
#elseif (Action == "Update")
    /// <summary>
    /// When the EntityName was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
#elseif (Action == "Delete")
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }
#endif

    // TODO: Add additional response properties specific to your EntityName entity
    // For example:
    // public string? Category { get; set; }
    // public decimal? Price { get; set; }
    // public string? CreatedByUser { get; set; }
}