// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestingDemo.Entities;

namespace TestingDemo.Api.Users.Commands;

/// <summary>
/// Command to update a user.
/// </summary>
public record UpdateUserCommand
{
    /// <summary>
    /// Id of the user.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// First name of the user.
    /// </summary>
    public string FirstName { get; set; }
    /// <summary>
    /// Last name of the user.
    /// </summary>
    public string LastName { get; set; }
    /// <summary>
    /// Email address of the user.
    /// </summary>
    public string Email { get; set; }
}

public class UpdateUserCommandValidator : Validator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty();
        RuleFor(x => x.LastName)
            .NotEmpty();
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}

public class UpdateUserCommandHandler : Endpoint<UpdateUserCommand,
                                   Results<Ok,
                                           NotFound,
                                           ProblemDetails>>
{
    private readonly ILogger<UpdateUserCommandHandler> _logger;
    private readonly IDemoDbContext _dbContext;

    public UpdateUserCommandHandler(
        ILogger<UpdateUserCommandHandler> logger,
        IDemoDbContext dbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public override void Configure()
    {
        Put("/api/users");
        Roles(Role.Admin.ToString());
        Description(x => x
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError));
    }

    public override async Task<Results<Ok, NotFound, ProblemDetails>> ExecuteAsync(
        UpdateUserCommand command, CancellationToken cancellationToken)
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

        bool emailExists = await _dbContext.Users
            .Where(u => u.Email == command.Email)
            .Where(u => u.Id != command.Id)
            .AnyAsync(cancellationToken);
        if (emailExists)
        {
            _logger.LogWarning(BaseURL, "User with email {Email} already exists.", command.Email);
            AddError(e => e.Email, "Email already exists.");
            return new ProblemDetails(ValidationFailures);
        }

        _logger.LogInformation("Updating user: {Id}", command.Id);

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.Email = command.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }
}
