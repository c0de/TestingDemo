// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestingDemo.Entities;

namespace TestingDemo.Api.Users.Queries;

/// <summary>
/// Get all active users.
/// </summary>
/// <remarks>
/// Example showing that we can return the results of a view.
/// </remarks>
public class GetActiveUsersQuery : EndpointWithoutRequest<IEnumerable<UserResponse>>
{
    private readonly IDemoDbContext _dbContext;

    public GetActiveUsersQuery(IDemoDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public override void Configure()
    {
        Get("/api/users/getactive");
        Description(x => x
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var users = await _dbContext.ActiveUsersView
            .Select(e => new UserResponse
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                CreatedAt = e.CreatedAt,
                IsActive = e.DeletedAt == null
            })
            .ToListAsync(cancellationToken);

        await Send.OkAsync(users, cancellationToken);
    }
}
