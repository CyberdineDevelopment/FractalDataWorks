using FractalDataWorks.Web.RestEndpoints.Base;
#if (EnablePagination)
using FractalDataWorks.Web.RestEndpoints.Pagination;
#endif
using FractalDataWorks.Results;

namespace TemplateNamespace.EntityName;

/// <summary>
/// Query endpoint for retrieving EntityName records using FractalDataWorks QueryEndpoint base class.
/// #if (EnablePagination)
/// Supports pagination for efficient data retrieval.
/// #endif
/// </summary>
#if (EnablePagination)
public class GetEntityNameEndpoint : QueryEndpoint<GetEntityNameRequest, EntityNameResponse>
#else
public class GetEntityNameEndpoint : FractalEndpoint<GetEntityNameRequest, List<EntityNameResponse>>
#endif
{
    public override void Configure()
    {
        Get("/api/EntityName");
#if (RequireAuthentication)
        // Configure authentication using FractalDataWorks patterns
        ConfigureEndpointSecurity();
#endif
        
        // Configure rate limiting using FractalDataWorks patterns
        ConfigureRateLimiting();
        
        Summary(s =>
        {
#if (EnablePagination)
            s.Summary = "Get all EntityName records with pagination";
            s.Description = "Retrieves a paginated list of EntityName records using FractalDataWorks QueryEndpoint";
#else
            s.Summary = "Get all EntityName records";
            s.Description = "Retrieves all EntityName records using FractalDataWorks FractalEndpoint";
#endif
            s.Responses[200] = "EntityName records retrieved successfully";
            s.Responses[400] = "Invalid request parameters";
#if (RequireAuthentication)
            s.Responses[401] = "Unauthorized access";
#endif
        });
    }
    
#if (RequireAuthentication)
    /// <summary>
    /// Configure authentication for this endpoint using FractalDataWorks security patterns.
    /// </summary>
    protected override void ConfigureEndpointSecurity()
    {
        // Example: Require authentication but allow any authenticated user
        Policies("RequireUser");
        
        // Or require specific roles:
        // Roles("User", "Admin");
        
        // Or use specific authentication schemes:
        // AuthSchemes("Bearer");
    }
#endif

#if (EnablePagination)
    protected override async Task<IGenericResult<PagedResponse<EntityNameResponse>>> ExecuteQueryAsync(GetEntityNameRequest query, CancellationToken ct)
#else
    protected override async Task<IGenericResult<List<EntityNameResponse>>> ExecuteQueryAsync(GetEntityNameRequest query, CancellationToken ct)
#endif
    {
        try
        {
            // TODO: Implement your data access logic using DataGateway
            // Example:
            // var dataCommand = new GetEntityNameCommand(query.Search);
            // var result = await DataGateway.Execute<List<EntityName>>(dataCommand, ct);
            // if (!result.IsSuccess)
            //     return GenericResult<PagedResponse<EntityNameResponse>>.Failure(result.Message);

            // For now, return sample data
            var sampleData = new List<EntityNameResponse>
            {
                new EntityNameResponse
                {
                    Id = 1,
                    Name = "Sample EntityName 1",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new EntityNameResponse
                {
                    Id = 2,
                    Name = "Sample EntityName 2",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(query.Search))
            {
                sampleData = sampleData.Where(x => x.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase))
                                     .ToList();
            }

#if (EnablePagination)
            // Apply pagination
            var totalCount = sampleData.Count;
            var pagedData = sampleData.Skip(query.Offset)
                                    .Take(query.PageSize)
                                    .ToList();

            var response = PagedResponse<EntityNameResponse>.Create(pagedData, query, totalCount);
            return GenericResult<PagedResponse<EntityNameResponse>>.Success(response);
#else
            return GenericResult<List<EntityNameResponse>>.Success(sampleData);
#endif
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving EntityName records");
#if (EnablePagination)
            return GenericResult<PagedResponse<EntityNameResponse>>.Failure("Failed to retrieve EntityName records");
#else
            return GenericResult<List<EntityNameResponse>>.Failure("Failed to retrieve EntityName records");
#endif
        }
    }
}