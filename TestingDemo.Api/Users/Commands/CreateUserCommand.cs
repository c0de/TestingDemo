// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Mail;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestingDemo.Entities;
using TestingDemo.Entities.Models;

namespace TestingDemo.Api.Users.Commands;

/// <summary>
/// Command to create a new user.
/// </summary>
public record CreateUserCommand
{
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
    /// <summary>
    /// Gets or sets the role assigned to the user.
    /// </summary>
    public Role Role { get; set; } = Role.User;
}

/// <summary>
/// Response for <see cref="CreateUserCommand"/>
/// </summary>
public class CreateUserCommandResponse
{
    /// <summary>
    /// Unique Identifier of the created user.
    /// </summary>
    public int Id { get; set; }
}

/// <summary>
/// Validator for <see cref="CreateUserCommand"/>
/// </summary>
public class CreateUserCommandValidator : Validator<CreateUserCommand>
{
    public CreateUserCommandValidator()
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

/// <summary>
/// Handler for <see cref="CreateUserCommand"/>
/// </summary>
public class CreateUserCommandHandler : Endpoint<CreateUserCommand,
                                   Results<Created<CreateUserCommandResponse>,
                                           NotFound,
                                           ProblemDetails>>
{
    private readonly ILogger<CreateUserCommandHandler> _logger;
    private readonly IDemoDbContext _dbContext;
    private readonly IEmailService _emailService;

    /// <summary>
    /// Create an instance of the handler.
    /// </summary>
    public CreateUserCommandHandler(
        ILogger<CreateUserCommandHandler> logger,
        IDemoDbContext dbContext,
        IEmailService emailService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    public override void Configure()
    {
        Post("/api/users");
        Roles(Role.Admin.ToString());
        Description(x => x
            .Produces<CreateUserCommandResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError));
    }

    public override async Task<Results<Created<CreateUserCommandResponse>, NotFound, ProblemDetails>> ExecuteAsync(
        CreateUserCommand command, CancellationToken cancellationToken)
    {
        bool userExists = await _dbContext.Users
            .AnyAsync(u => u.Email == command.Email, cancellationToken);
        if (userExists)
        {
            _logger.LogWarning(BaseURL, "User with email {Email} already exists.", command.Email);
            AddError(e => e.Email, "Email already exists.");
            return new ProblemDetails(ValidationFailures);
        }

        _logger.LogInformation("Creating user with email: {Email}", command.Email);

        var user = new User
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            Role = command.Role.ToString(),
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await AssignDefaultDashboardsAsync(user, cancellationToken);
        await NotifyUserAsync(user, cancellationToken);

        var response = new CreateUserCommandResponse
        {
            Id = user.Id
        };

        return TypedResults.Created("/api/users/1", response);
    }

    private async Task AssignDefaultDashboardsAsync(User user, CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken); // Simulate async operation
    }

    private async Task NotifyUserAsync(User user, CancellationToken cancellationToken)
    {
        var message = new MailMessage
        {
            From = new MailAddress("noreply@test.com"),
            To = { new MailAddress(user.Email) },
            Subject = "Welcome to TestingDemo",
            Body = $"Hello {user.FirstName},\n\nWelcome to TestingDemo!",
        };

        await _emailService.SendEmailAsync(message, cancellationToken);
    }
}
