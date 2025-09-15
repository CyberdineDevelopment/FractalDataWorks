using FastEndpoints;
using FractalDataWorks.Web.RestEndpoints.Base;
using FractalDataWorks.Results;

namespace TemplateNamespace.EntityName;

/// <summary>
/// Command endpoint for CommandAction EntityName operations.
/// Implements CQRS pattern with validation and authorization.
/// </summary>
public class CommandActionEntityNameEndpoint : CommandEndpoint<CommandActionEntityNameRequest, CommandActionEntityNameResponse>
{
    public override void Configure()
    {
#if (Action == "Create")
        Post("/api/EntityName");
#elseif (Action == "Update")
        Put("/api/EntityName/{id}");
#elseif (Action == "Delete")
        Delete("/api/EntityName/{id}");
#endif

#if (RequireAuthentication)
        // Configure authentication using FractalDataWorks patterns
        ConfigureEndpointSecurity();
#endif
        
        // Configure rate limiting using FractalDataWorks patterns
        ConfigureRateLimiting();

        Summary(s =>
        {
#if (Action == "Create")
            s.Summary = "Create a new EntityName";
            s.Description = "Creates a new EntityName record using FractalDataWorks CommandEndpoint";
            s.Responses[201] = "EntityName created successfully";
            s.Responses[400] = "Invalid EntityName data";
            s.Responses[409] = "EntityName already exists";
#elseif (Action == "Update")
            s.Summary = "Update an existing EntityName";
            s.Description = "Updates an existing EntityName record using FractalDataWorks CommandEndpoint";
            s.Responses[200] = "EntityName updated successfully";
            s.Responses[400] = "Invalid EntityName data";
            s.Responses[404] = "EntityName not found";
#elseif (Action == "Delete")
            s.Summary = "Delete an EntityName";
            s.Description = "Deletes an EntityName record using FractalDataWorks CommandEndpoint";
            s.Responses[204] = "EntityName deleted successfully";
            s.Responses[404] = "EntityName not found";
#endif
#if (RequireAuthentication)
            s.Responses[401] = "Unauthorized access";
            s.Responses[403] = "Insufficient permissions";
#endif
        });

        Validator<CommandActionEntityNameRequestValidator>();
    }

    protected override async Task<IFdwResult<CommandActionEntityNameResponse>> ExecuteCommandAsync(CommandActionEntityNameRequest command, CancellationToken ct)
    {
        try
        {
#if (Action == "Create")
            // TODO: Check if EntityName already exists
            // var existsResult = await DataGateway.Execute(new CheckEntityNameExistsCommand(command.Name), ct);
            // if (existsResult.IsSuccess && existsResult.Value)
            // {
            //     return FdwResult<CommandActionEntityNameResponse>.Failure("EntityName already exists");
            // }

            // TODO: Create EntityName using DataGateway
            // var createCommand = new CreateEntityNameCommand(command.Name, command.Description);
            // var createResult = await DataGateway.Execute<EntityName>(createCommand, ct);
            // if (!createResult.IsSuccess)
            // {
            //     return FdwResult<CommandActionEntityNameResponse>.Failure(createResult.Message);
            // }

            // For now, simulate creation
            var newEntity = new CommandActionEntityNameResponse
            {
                Id = Random.Shared.Next(1000, 9999),
                Name = command.Name,
                CreatedAt = DateTime.UtcNow
            };

            Logger.LogInformation("EntityName created: {EntityId} - {EntityName}", newEntity.Id, newEntity.Name);
#elseif (Action == "Update")
            // TODO: Check if EntityName exists
            // var existsResult = await DataGateway.Execute(new GetEntityNameByIdCommand(command.Id), ct);
            // if (!existsResult.IsSuccess)
            // {
            //     return FdwResult<CommandActionEntityNameResponse>.Failure("EntityName not found");
            // }

            // TODO: Update EntityName using DataGateway
            // var updateCommand = new UpdateEntityNameCommand(command.Id, command.Name, command.Description);
            // var updateResult = await DataGateway.Execute<EntityName>(updateCommand, ct);
            // if (!updateResult.IsSuccess)
            // {
            //     return FdwResult<CommandActionEntityNameResponse>.Failure(updateResult.Message);
            // }

            // For now, simulate update
            var updatedEntity = new CommandActionEntityNameResponse
            {
                Id = command.Id,
                Name = command.Name,
                UpdatedAt = DateTime.UtcNow
            };

            Logger.LogInformation("EntityName updated: {EntityId} - {EntityName}", updatedEntity.Id, updatedEntity.Name);
#elseif (Action == "Delete")
            // TODO: Check if EntityName exists
            // var existsResult = await DataGateway.Execute(new GetEntityNameByIdCommand(command.Id), ct);
            // if (!existsResult.IsSuccess)
            // {
            //     return FdwResult<CommandActionEntityNameResponse>.Failure("EntityName not found");
            // }

            // TODO: Delete EntityName using DataGateway
            // var deleteCommand = new DeleteEntityNameCommand(command.Id);
            // var deleteResult = await DataGateway.Execute(deleteCommand, ct);
            // if (!deleteResult.IsSuccess)
            // {
            //     return FdwResult<CommandActionEntityNameResponse>.Failure(deleteResult.Message);
            // }

            Logger.LogInformation("EntityName deleted: {EntityId}", command.Id);

            // For delete operations, return success without data
            var deleteResponse = new CommandActionEntityNameResponse
            {
                Id = command.Id,
                Success = true
            };
#endif

#if (Action == "Create")
            return FdwResult<CommandActionEntityNameResponse>.Success(newEntity);
#elseif (Action == "Update")
            return FdwResult<CommandActionEntityNameResponse>.Success(updatedEntity);
#elseif (Action == "Delete")
            return FdwResult<CommandActionEntityNameResponse>.Success(deleteResponse);
#endif
        }
        catch (Exception ex)
        {
#if (Action == "Create")
            Logger.LogError(ex, "Error creating EntityName: {EntityName}", command.Name);
            return FdwResult<CommandActionEntityNameResponse>.Failure("Failed to create EntityName");
#elseif (Action == "Update")
            Logger.LogError(ex, "Error updating EntityName: {EntityId} - {EntityName}", command.Id, command.Name);
            return FdwResult<CommandActionEntityNameResponse>.Failure("Failed to update EntityName");
#elseif (Action == "Delete")
            Logger.LogError(ex, "Error deleting EntityName: {EntityId}", command.Id);
            return FdwResult<CommandActionEntityNameResponse>.Failure("Failed to delete EntityName");
#endif
        }
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
#if (RequireAdminRole)
        Roles("Admin");
#else
        // Roles("User", "Admin");
#endif
        
        // Or use specific authentication schemes:
        // AuthSchemes("Bearer");
    }
#endif
}