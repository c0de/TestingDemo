using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestingDemo.Entities;

namespace TestingDemo.Api.Dashboards.Queries;

/// <summary>
/// Get all dashboards.
/// </summary>
public class GetUsersQuery : EndpointWithoutRequest<IEnumerable<DashboardResponse>>
{
    private readonly IDemoDbContext _dbContext;

    public GetUsersQuery(IDemoDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public override void Configure()
    {
        Get("/api/dashboards");
        Description(x => x
            .Produces<DashboardResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var users = await _dbContext.Dashboards
            .AsNoTracking()
            .Where(e => e.DeletedAt == null)
            .Select(e => new DashboardResponse
            {
                Id = e.Id,
                Name = e.Name,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync(cancellationToken);

        await Send.OkAsync(users, cancellationToken);
    }
}
