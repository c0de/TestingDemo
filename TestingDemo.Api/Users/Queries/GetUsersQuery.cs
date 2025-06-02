using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestingDemo.Entities;

namespace TestingDemo.Api.Users.Queries;

/// <summary>
/// Get all active users.
/// </summary>
public class GetUsersQuery : EndpointWithoutRequest<IEnumerable<UserResponse>>
{
    private readonly IRepository _dbContext;

    public GetUsersQuery(IRepository dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public override void Configure()
    {
        Get("/api/users");
        Description(x => x
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(e => e.DeletedAt == null)
            .Select(e => new UserResponse
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync(cancellationToken);

        await SendOkAsync(users, cancellationToken);
    }
}
