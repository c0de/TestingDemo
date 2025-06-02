using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestingDemo.Api.Users.Queries;
using TestingDemo.Entities;

namespace TestingDemo.Api.Users.Commands;

/// <summary>
/// Command to delete a user.
/// </summary>
public record DeleteUserCommand
{
    /// <summary>
    /// Id of the user.
    /// </summary>
    public int Id { get; set; }
}

public class DeleteUserCommandHandler : Endpoint<DeleteUserCommand,
                                   Results<Ok,
                                           NotFound,
                                           ProblemDetails>>
{
    private readonly ILogger<DeleteUserCommandHandler> _logger;
    private readonly IRepository _dbContext;

    public DeleteUserCommandHandler(
        ILogger<DeleteUserCommandHandler> logger,
        IRepository dbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public override void Configure()
    {
        Delete("/api/users/{Id}");
        Roles("Admin");
        Description(x => x
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError));
    }

    public override async Task<Results<Ok, NotFound, ProblemDetails>> ExecuteAsync(
        DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Where(u => u.Id == command.Id)
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
        if (user == null)
        {
            _logger.LogWarning(BaseURL, "User with id {Id} not found.", command.Id);
            return TypedResults.NotFound();
        }

        user.DeletedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }
}