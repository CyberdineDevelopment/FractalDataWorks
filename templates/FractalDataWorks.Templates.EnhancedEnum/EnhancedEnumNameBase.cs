namespace TemplateNamespace;

/// <summary>
/// Base implementation for EnhancedEnumName Enhanced Enum.
/// Provides common functionality for all EnhancedEnumName options.
/// </summary>
public abstract class EnhancedEnumNameBase : IEnhancedEnumName
{
    /// <summary>
    /// Initializes a new instance of EnhancedEnumNameBase.
    /// </summary>
    /// <param name="id">The unique identifier for this option.</param>
    /// <param name="name">The display name of this option.</param>
#if (IncludeDescription && IncludeSort)
    /// <param name="description">The description of this option.</param>
    /// <param name="sortOrder">The sort order for this option.</param>
    /// <param name="isActive">Whether this option is active.</param>
    protected EnhancedEnumNameBase(int id, string name, string description, int sortOrder, bool isActive = true)
#elseif (IncludeDescription)
    /// <param name="description">The description of this option.</param>
    /// <param name="isActive">Whether this option is active.</param>
    protected EnhancedEnumNameBase(int id, string name, string description, bool isActive = true)
#elseif (IncludeSort)
    /// <param name="sortOrder">The sort order for this option.</param>
    /// <param name="isActive">Whether this option is active.</param>
    protected EnhancedEnumNameBase(int id, string name, int sortOrder, bool isActive = true)
#else
    /// <param name="isActive">Whether this option is active.</param>
    protected EnhancedEnumNameBase(int id, string name, bool isActive = true)
#endif
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
#if (IncludeDescription)
        Description = description ?? throw new ArgumentNullException(nameof(description));
#endif
#if (IncludeSort)
        SortOrder = sortOrder;
#endif
        IsActive = isActive;
    }

    /// <inheritdoc />
    public int Id { get; }

    /// <inheritdoc />
    public string Name { get; }

#if (IncludeDescription)
    /// <inheritdoc />
    public string Description { get; }
#endif

#if (IncludeSort)
    /// <inheritdoc />
    public int SortOrder { get; }
#endif

    /// <inheritdoc />
    public bool IsActive { get; }

    /// <summary>
    /// Returns the name of this EnhancedEnumName option.
    /// </summary>
    public override string ToString() => Name;

    /// <summary>
    /// Determines whether the specified object is equal to the current EnhancedEnumName.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not IEnhancedEnumName other) return false;
        return Id == other.Id;
    }

    /// <summary>
    /// Returns a hash code for this EnhancedEnumName.
    /// </summary>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Equality operator for EnhancedEnumName comparison.
    /// </summary>
    public static bool operator ==(EnhancedEnumNameBase? left, EnhancedEnumNameBase? right)
    {
        return ReferenceEquals(left, right) || (left?.Equals(right) == true);
    }

    /// <summary>
    /// Inequality operator for EnhancedEnumName comparison.
    /// </summary>
    public static bool operator !=(EnhancedEnumNameBase? left, EnhancedEnumNameBase? right)
    {
        return !(left == right);
    }

    // TODO: Add additional methods or properties specific to your EnhancedEnumName
    // For example:
    // public virtual bool CanTransitionTo(IEnhancedEnumName target) => true;
    // public virtual string GetCssClass() => Name.ToLowerInvariant();
    // public virtual bool IsValidForUser(string userId) => IsActive;
}