namespace TemplateNamespace;

/// <summary>
/// OptionName implementation of Enhanced Enum.
/// Description
/// </summary>
public sealed class OptionName : BaseClassName
{
    /// <summary>
    /// Initializes a new instance of the OptionName Enhanced Enum option.
    /// </summary>
    public OptionName() 
        : base(
            id: OptionId, 
            name: "DisplayName", 
            description: "Description", 
            sortOrder: SortOrder, 
            isActive: IsActiveByDefault)
    {
    }

    // TODO: Add specific properties or methods for OptionName
    // For example:
    // public override string GetCssClass() => "option-optionname";
    // public override bool CanTransitionTo(IEnhancedEnum target) => target.Id != OptionId;
    // public string Color => "#007bff"; // Primary blue
    // public string IconClass => "fas fa-star";
    // public bool RequiresApproval => true;
    // public string[] AllowedRoles => new[] { "Admin", "Manager" };
}