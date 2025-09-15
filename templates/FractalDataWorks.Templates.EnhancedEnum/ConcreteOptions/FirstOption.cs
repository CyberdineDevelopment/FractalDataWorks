namespace TemplateNamespace;

/// <summary>
/// FirstOption implementation of EnhancedEnumName Enhanced Enum.
/// </summary>
public sealed class FirstOption : EnhancedEnumNameBase
{
    /// <summary>
    /// Initializes a new instance of the FirstOption EnhancedEnumName.
    /// </summary>
    public FirstOption() 
#if (IncludeDescription && IncludeSort)
        : base(id: 1, name: "FirstOption", description: "FirstOption EnhancedEnumName option", sortOrder: 1)
#elseif (IncludeDescription)
        : base(id: 1, name: "FirstOption", description: "FirstOption EnhancedEnumName option")
#elseif (IncludeSort)
        : base(id: 1, name: "FirstOption", sortOrder: 1)
#else
        : base(id: 1, name: "FirstOption")
#endif
    {
    }

    // TODO: Add specific properties or methods for FirstOption
    // For example:
    // public override string GetCssClass() => "status-active";
    // public override bool CanTransitionTo(IEnhancedEnumName target) => target.Id != 1; // Can't transition to self
    // public string Color => "#28a745"; // Success green
    // public string IconClass => "fas fa-check-circle";
}