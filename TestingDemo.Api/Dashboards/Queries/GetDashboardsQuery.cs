// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestingDemo.Entities;

namespace TestingDemo.Api.Dashboards.Queries;

/// <summary>
/// Get all dashboards.
/// </summary>
public class GetDashboardsQuery : EndpointWithoutRequest<IEnumerable<DashboardResponse>>
{
    private readonly IDemoDbContext _dbContext;

    public GetDashboardsQuery(IDemoDbContext dbContext)
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
        var dashboards = await _dbContext.Dashboards
            .AsNoTracking()
            .Where(e => e.DeletedAt == null)
            .Select(e => new DashboardResponse
            {
                Id = e.Id,
                Name = e.Name,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync(cancellationToken);

        await Send.OkAsync(dashboards, cancellationToken);
    }
}
