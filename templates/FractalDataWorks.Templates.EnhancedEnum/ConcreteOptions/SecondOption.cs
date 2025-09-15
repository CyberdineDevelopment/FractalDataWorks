namespace TemplateNamespace;

/// <summary>
/// SecondOption implementation of EnhancedEnumName Enhanced Enum.
/// </summary>
public sealed class SecondOption : EnhancedEnumNameBase
{
    /// <summary>
    /// Initializes a new instance of the SecondOption EnhancedEnumName.
    /// </summary>
    public SecondOption() 
#if (IncludeDescription && IncludeSort)
        : base(id: 2, name: "SecondOption", description: "SecondOption EnhancedEnumName option", sortOrder: 2)
#elseif (IncludeDescription)
        : base(id: 2, name: "SecondOption", description: "SecondOption EnhancedEnumName option")
#elseif (IncludeSort)
        : base(id: 2, name: "SecondOption", sortOrder: 2)
#else
        : base(id: 2, name: "SecondOption")
#endif
    {
    }

    // TODO: Add specific properties or methods for SecondOption
    // For example:
    // public override string GetCssClass() => "status-inactive";
    // public override bool CanTransitionTo(IEnhancedEnumName target) => true; // Can transition to any status
    // public string Color => "#6c757d"; // Secondary gray
    // public string IconClass => "fas fa-pause-circle";
}