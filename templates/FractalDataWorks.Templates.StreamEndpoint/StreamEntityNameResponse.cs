namespace TemplateNamespace.EntityName;

/// <summary>
/// Response model for streamed EntityName data.
/// </summary>
public class StreamEntityNameResponse
{
    /// <summary>
    /// The unique identifier of the EntityName.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the EntityName.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The description of the EntityName.
    /// </summary>
    public string? Description { get; set; }

#if (EnableFilters)
    /// <summary>
    /// The category of the EntityName.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Whether the EntityName is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When the EntityName was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
#endif

    /// <summary>
    /// The timestamp when this item was streamed.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// The sequence number of this item in the stream.
    /// </summary>
    public long SequenceNumber { get; set; }

    /// <summary>
    /// Optional metadata about the stream item.
    /// </summary>
    public string? StreamMetadata { get; set; }

    // TODO: Add additional response properties specific to your EntityName entity
    // For example:
    // public string? Status { get; set; }
    // public decimal? Price { get; set; }
    // public string? LastModifiedByUser { get; set; }
    // public Dictionary<string, object>? AdditionalData { get; set; }
}