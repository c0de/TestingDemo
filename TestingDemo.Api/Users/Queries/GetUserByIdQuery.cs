using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestingDemo.Entities;

namespace TestingDemo.Api.Users.Queries;

public class GetUserByIdQuery
{
    public int Id { get; set; }
}

public class GetUserByIdQueryHandler : Endpoint<GetUserByIdQuery, UserResponse>
{
    private readonly IDemoDbContext _dbContext;

    public GetUserByIdQueryHandler(IDemoDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public override void Configure()
    {
        Get("/api/users/{Id}");
        Description(x => x
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(e => e.Id == query.Id)
            .Where(e => e.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }
        var response = new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
        await Send.OkAsync(response, cancellationToken);
    }
}
