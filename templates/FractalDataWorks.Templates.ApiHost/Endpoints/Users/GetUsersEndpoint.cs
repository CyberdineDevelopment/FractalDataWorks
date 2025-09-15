using FastEndpoints;
using FractalDataWorks.Web.RestEndpoints.Base;
using FractalDataWorks.Web.RestEndpoints.Pagination;
using FractalDataWorks.Results;

namespace FractalDataWorksApiHost.Endpoints.Users;

/// <summary>
/// Example Query endpoint that retrieves users with pagination.
/// Demonstrates the QueryEndpoint base class usage.
/// </summary>
public class GetUsersEndpoint : QueryEndpoint<GetUsersRequest, UserResponse>
{
    public override void Configure()
    {
        Get("/api/users");
        AllowAnonymous(); // Change to Policies("RequireUser") for authentication
        Summary(s =>
        {
            s.Summary = "Get all users with pagination";
            s.Description = "Retrieves a paginated list of users from the system";
            s.Responses[200] = "Users retrieved successfully";
            s.Responses[400] = "Invalid pagination parameters";
        });
    }

    protected override async Task<IFdwResult<PagedResponse<UserResponse>>> ExecuteQueryAsync(GetUsersRequest query, CancellationToken ct)
    {
        try
        {
            // Example implementation - replace with actual data access
            var users = new List<UserResponse>();
            
            // Simulate database query with DataGateway
            // var usersResult = await DataGateway.Execute(new GetUsersCommand(query.Search), ct);
            // if (!usersResult.IsSuccess) return usersResult;
            
            // For demo purposes, create sample data
            for (int i = 1; i <= 10; i++)
            {
                users.Add(new UserResponse
                {
                    Id = i,
                    Name = $"User {i}",
                    Email = $"user{i}@example.com",
                    CreatedAt = DateTime.UtcNow.AddDays(-i)
                });
            }

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(query.Search))
            {
                users = users.Where(u => u.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                                        u.Email.Contains(query.Search, StringComparison.OrdinalIgnoreCase))
                            .ToList();
            }

            // Apply pagination
            var totalCount = users.Count;
            var pagedUsers = users.Skip(query.Offset)
                                 .Take(query.PageSize)
                                 .ToList();

            var response = PagedResponse<UserResponse>.Create(pagedUsers, query, totalCount);
            return FdwResult<PagedResponse<UserResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving users");
            return FdwResult<PagedResponse<UserResponse>>.Failure("Failed to retrieve users");
        }
    }
}

/// <summary>
/// Request model for getting users with pagination and search.
/// </summary>
public class GetUsersRequest : PagedRequest
{
    /// <summary>
    /// Optional search term to filter users by name or email.
    /// </summary>
    public string? Search { get; set; }
}