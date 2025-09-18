// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using TestingDemo.Entities;

namespace TestingDemo.Api.Users.Commands;

/// <summary>
/// Command to process users
/// </summary>
public record ProcessUsersCommand
{
    /// <summary>
    /// Job detail identifier
    /// </summary>
    public int JobDetailId { get; set; }
}

/// <summary>
/// Handler for <see cref="ProcessUsersCommand"/>
/// </summary>
public class ProcessUsersCommandHandler : Endpoint<ProcessUsersCommand,
                                   Results<Ok<bool>,
                                           ProblemDetails>>
{
    private readonly ILogger<ProcessUsersCommandHandler> _logger;
    private readonly IDemoDbContext _dbContext;

    /// <summary>
    /// Create an instance of the handler.
    /// </summary>
    public ProcessUsersCommandHandler(
        ILogger<ProcessUsersCommandHandler> logger,
        IDemoDbContext dbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public override void Configure()
    {
        Post("/api/users/process");
        Roles(Role.Admin.ToString());
        Description(x => x
            .Produces<ProcessUsersCommand>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError));
    }

    public override async Task<Results<Ok<bool>, ProblemDetails>> ExecuteAsync(
        ProcessUsersCommand command, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _logger.LogInformation($"Starting {nameof(ProcessUsersCommand)} at {DateTime.UtcNow}");

            stopwatch.Start();

            var commandText = "EXEC dbo.Process_Users @JobDetailId";
            var sqlParameters = new SqlParameter[] {
                new("JobDetailId", command.JobDetailId),
            };

            await _dbContext.ExecuteAsync(commandText, 60, sqlParameters, cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation($"Finished {nameof(ProcessUsersCommand)} in {stopwatch.ElapsedMilliseconds} ms");

            return TypedResults.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while processing users: {ex.Message} in {stopwatch.ElapsedMilliseconds}");

            return new ProblemDetails
            {
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            };
        }
    }
}
