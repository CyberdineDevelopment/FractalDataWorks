using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services.Scheduling.Abstractions.EnhancedEnums;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Collection of trigger types for scheduling system.
/// </summary>
[EnumCollection(typeof(TriggerTypeBase), typeof(TriggerTypeBase), typeof(TriggerTypes))]
public abstract class TriggerTypes : EnumCollectionBase<TriggerTypeBase>
{
    // DO NOT IMPLEMENT BY HAND!
    // Source generator automatically creates static TriggerTypes class with:
    // - TriggerTypes.Cron (returns TriggerTypeBase)
    // - TriggerTypes.Interval (returns TriggerTypeBase)
    // - TriggerTypes.Manual (returns TriggerTypeBase)
    // - TriggerTypes.Once (returns TriggerTypeBase)
    // - TriggerTypes.All (collection of TriggerTypeBase)
    // - TriggerTypes.GetById(int id) (returns TriggerTypeBase)
    // - TriggerTypes.GetByName(string name) (returns TriggerTypeBase)
}