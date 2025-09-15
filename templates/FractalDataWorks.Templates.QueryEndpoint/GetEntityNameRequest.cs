#if (EnablePagination)
using FractalDataWorks.Web.RestEndpoints.Pagination;
#endif

namespace TemplateNamespace.EntityName;

/// <summary>
/// Request model for retrieving EntityName records.
/// </summary>
#if (EnablePagination)
public class GetEntityNameRequest : PagedRequest
#else
public class GetEntityNameRequest
#endif
{
    /// <summary>
    /// Optional search term to filter EntityName records.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Optional filter by active status.
    /// </summary>
    public bool? IsActive { get; set; }

    // TODO: Add additional query parameters specific to your EntityName entity
    // For example:
    // public DateTime? CreatedAfter { get; set; }
    // public DateTime? CreatedBefore { get; set; }
    // public string? Category { get; set; }
}