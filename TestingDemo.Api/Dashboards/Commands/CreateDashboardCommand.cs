using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestingDemo.Entities;
using TestingDemo.Entities.Models;

namespace TestingDemo.Api.Dashboards.Commands;

/// <summary>
/// Command to create a new dashboard.
/// </summary>
public record CreateDashboardCommand
{
    /// <summary>
    /// Name of the dashboard.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Description of the dashboard.
    /// </summary>
    public string Description { get; set; }
}

/// <summary>
/// Response for <see cref="CreateDashboardCommand"/>
/// </summary>
public class CreateDashboardCommandResponse
{
    /// <summary>
    /// Unique Identifier of the created dashboard.
    /// </summary>
    public int Id { get; set; }
}

/// <summary>
/// Validator for <see cref="CreateDashboardCommand"/>
/// </summary>
public class CreateDashboardCommandValidator : Validator<CreateDashboardCommand>
{
    private readonly IDemoDbContext _dbContext;

    public CreateDashboardCommandValidator(IDemoDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .MustAsync(async (name, cancellationToken) =>
            {
                var exists = await _dbContext.Dashboards
                    .AnyAsync(d => d.Name == name && d.DeletedAt == null, cancellationToken);
                return !exists;
            })
            .WithMessage("Dashboard name already exists.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

/// <summary>
/// Handler for <see cref="CreateDashboardCommand"/>
/// </summary>
public class CreateDashboardCommandHandler : Endpoint<CreateDashboardCommand,
                                   Results<Created<CreateDashboardCommandResponse>,
                                           NotFound,
                                           ProblemDetails>>
{
    private readonly ILogger<CreateDashboardCommandHandler> _logger;
    private readonly IDemoDbContext _dbContext;

    /// <summary>
    /// Create an instance of the handler.
    /// </summary>
    public CreateDashboardCommandHandler(
        ILogger<CreateDashboardCommandHandler> logger,
        IDemoDbContext dbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public override void Configure()
    {
        Post("/api/dashboards");
        Roles(Role.Admin.ToString());
        Description(x => x
            .Produces<CreateDashboardCommandResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError));
    }

    public override async Task<Results<Created<CreateDashboardCommandResponse>, NotFound, ProblemDetails>> ExecuteAsync(
        CreateDashboardCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating dashboard with name: {Name}", command.Name);

        var dashboard = new Dashboard
        {
            Name = command.Name,
            Description = command.Description,
        };
        _dbContext.Dashboards.Add(dashboard);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new CreateDashboardCommandResponse
        {
            Id = dashboard.Id
        };

        return TypedResults.Created($"/api/dashboards/{dashboard.Id}", response);
    }
}
