using FastEndpoints;
using FractalDataWorks.Web.RestEndpoints.Base;
using FractalDataWorks.Results;

namespace FractalDataWorksApiHost.Endpoints.Users;

/// <summary>
/// Example Command endpoint that creates a new user.
/// Demonstrates the CommandEndpoint base class usage.
/// </summary>
public class CreateUserEndpoint : CommandEndpoint<CreateUserRequest, CreateUserResponse>
{
    public override void Configure()
    {
        Post("/api/users");
//#if (AuthenticationType != "None")
        Policies("RequireUser");
//#else
        AllowAnonymous();
//#endif
        Summary(s =>
        {
            s.Summary = "Create a new user";
            s.Description = "Creates a new user in the system";
            s.Responses[201] = "User created successfully";
            s.Responses[400] = "Invalid user data";
            s.Responses[409] = "User with email already exists";
        });
    }

    protected override async Task<IFdwResult<CreateUserResponse>> ExecuteCommandAsync(CreateUserRequest command, CancellationToken ct)
    {
        try
        {
            // Validate the request
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                return FdwResult<CreateUserResponse>.Failure("Name is required");
            }

            if (string.IsNullOrWhiteSpace(command.Email))
            {
                return FdwResult<CreateUserResponse>.Failure("Email is required");
            }

            // Example implementation - replace with actual data access
            // Check if user already exists
            // var existingUser = await DataGateway.Execute(new GetUserByEmailCommand(command.Email), ct);
            // if (existingUser.IsSuccess)
            // {
            //     return FdwResult<CreateUserResponse>.Failure("User with this email already exists");
            // }

            // Create new user
            // var createResult = await DataGateway.Execute(new CreateUserCommand(command.Name, command.Email), ct);
            // if (!createResult.IsSuccess)
            // {
            //     return FdwResult<CreateUserResponse>.Failure(createResult.Message);
            // }

            // For demo purposes, simulate user creation
            var newUser = new CreateUserResponse
            {
                Id = Random.Shared.Next(1000, 9999),
                Name = command.Name,
                Email = command.Email,
                CreatedAt = DateTime.UtcNow
            };

            Logger.LogInformation("User created: {UserId} - {UserName}", newUser.Id, newUser.Name);

            return FdwResult<CreateUserResponse>.Success(newUser);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating user: {UserName} - {UserEmail}", command.Name, command.Email);
            return FdwResult<CreateUserResponse>.Failure("Failed to create user");
        }
    }

//#if (AuthenticationType != "None")
    protected override string[] GetRequiredRoles() => new[] { "User", "Admin" };
//#endif
}

/// <summary>
/// Request model for creating a new user.
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// The user's full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The user's email address (must be unique).
    /// </summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Response model for user creation.
/// </summary>
public class CreateUserResponse
{
    /// <summary>
    /// The newly created user's ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The user's full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// When the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}