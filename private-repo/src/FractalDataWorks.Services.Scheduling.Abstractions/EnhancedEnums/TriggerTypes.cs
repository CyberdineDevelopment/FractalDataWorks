using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Scheduling.Abstractions.EnhancedEnums;

/// <summary>
/// Collection of trigger types for scheduling system.
/// </summary>
[TypeCollection(typeof(TriggerTypeBase), typeof(ITriggerType), typeof(TriggerTypes))]
public abstract partial class TriggerTypes : TypeCollectionBase<TriggerTypeBase, ITriggerType>
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