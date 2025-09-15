using System.Collections;

namespace TemplateNamespace;

/// <summary>
/// Collection base for EnhancedEnumName Enhanced Enum.
/// Provides static access to all EnhancedEnumName options and collection operations.
/// </summary>
#if (IncludeGlobalCollection)
[GlobalCollection]
#endif
public static class EnhancedEnumNameCollectionBase
{
    private static readonly Lazy<IReadOnlyList<IEnhancedEnumName>> _allItems = new(() => 
        DiscoverAllOptions().ToList().AsReadOnly());

    private static readonly Lazy<Dictionary<int, IEnhancedEnumName>> _itemsById = new(() =>
        All.ToDictionary(x => x.Id, StringComparer.Ordinal));

    private static readonly Lazy<Dictionary<string, IEnhancedEnumName>> _itemsByName = new(() =>
        All.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Gets all available EnhancedEnumName options.
    /// </summary>
    public static IReadOnlyList<IEnhancedEnumName> All => _allItems.Value;

    /// <summary>
    /// Gets all active EnhancedEnumName options.
    /// </summary>
    public static IEnumerable<IEnhancedEnumName> Active => All.Where(x => x.IsActive);

#if (IncludeSort)
    /// <summary>
    /// Gets all EnhancedEnumName options ordered by sort order.
    /// </summary>
    public static IEnumerable<IEnhancedEnumName> Ordered => All.OrderBy(x => x.SortOrder).ThenBy(x => x.Name);

    /// <summary>
    /// Gets all active EnhancedEnumName options ordered by sort order.
    /// </summary>
    public static IEnumerable<IEnhancedEnumName> ActiveOrdered => Active.OrderBy(x => x.SortOrder).ThenBy(x => x.Name);
#endif

    /// <summary>
    /// Gets a EnhancedEnumName option by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <returns>The EnhancedEnumName option, or null if not found.</returns>
    public static IEnhancedEnumName? GetById(int id)
    {
        return _itemsById.Value.TryGetValue(id, out var item) ? item : null;
    }

    /// <summary>
    /// Gets a EnhancedEnumName option by its name.
    /// </summary>
    /// <param name="name">The name of the option (case-insensitive).</param>
    /// <returns>The EnhancedEnumName option, or null if not found.</returns>
    public static IEnhancedEnumName? GetByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        return _itemsByName.Value.TryGetValue(name, out var item) ? item : null;
    }

    /// <summary>
    /// Checks if a EnhancedEnumName option with the specified ID exists.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <returns>True if the option exists, false otherwise.</returns>
    public static bool Exists(int id) => _itemsById.Value.ContainsKey(id);

    /// <summary>
    /// Checks if a EnhancedEnumName option with the specified name exists.
    /// </summary>
    /// <param name="name">The name of the option (case-insensitive).</param>
    /// <returns>True if the option exists, false otherwise.</returns>
    public static bool Exists(string name) => !string.IsNullOrWhiteSpace(name) && _itemsByName.Value.ContainsKey(name);

    /// <summary>
    /// Gets all EnhancedEnumName options that match the specified predicate.
    /// </summary>
    /// <param name="predicate">The condition to match.</param>
    /// <returns>A collection of matching EnhancedEnumName options.</returns>
    public static IEnumerable<IEnhancedEnumName> Where(Func<IEnhancedEnumName, bool> predicate)
    {
        return All.Where(predicate);
    }

    /// <summary>
    /// Converts the collection to a dictionary with ID as the key.
    /// </summary>
    /// <returns>A dictionary of EnhancedEnumName options keyed by ID.</returns>
    public static Dictionary<int, IEnhancedEnumName> ToDictionaryById()
    {
        return new Dictionary<int, IEnhancedEnumName>(_itemsById.Value);
    }

    /// <summary>
    /// Converts the collection to a dictionary with name as the key.
    /// </summary>
    /// <returns>A dictionary of EnhancedEnumName options keyed by name.</returns>
    public static Dictionary<string, IEnhancedEnumName> ToDictionaryByName()
    {
        return new Dictionary<string, IEnhancedEnumName>(_itemsByName.Value, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Discovers all EnhancedEnumName implementations using reflection.
    /// </summary>
    /// <returns>All concrete EnhancedEnumName implementations.</returns>
    private static IEnumerable<IEnhancedEnumName> DiscoverAllOptions()
    {
        var enumType = typeof(IEnhancedEnumName);
        var assembly = enumType.Assembly;

        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && enumType.IsAssignableFrom(t))
            .Select(t => Activator.CreateInstance(t))
            .Cast<IEnhancedEnumName>()
            .OrderBy(x => x.Id);
    }

    // TODO: Add additional collection methods specific to your EnhancedEnumName
    // For example:
    // public static IEnumerable<IEnhancedEnumName> GetByCategory(string category) => All.Where(x => x.Category == category);
    // public static bool CanTransition(int fromId, int toId) => GetById(fromId)?.CanTransitionTo(GetById(toId)) == true;
    // public static IEnumerable<IEnhancedEnumName> GetAvailableFor(string userId) => Active.Where(x => x.IsValidForUser(userId));
}