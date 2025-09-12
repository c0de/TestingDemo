using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TestingDemo.Api.Dashboards.Commands;

namespace TestingDemo.Tests.Dashboards;

/// <summary>
/// Testing <see cref="CreateDashboardCommand"/>.
/// </summary>
public class CreateDashboardTests
{
    /// <summary>
    /// Ensure an anonymous user is unauthorized.
    /// </summary>
    [Fact]
    public async Task Anonymous_ShouldFail()
    {
        // Arrange
        var session = await TestingFactory.CreateAnonymousAsync();

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/dashboards", new { });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Ensure an admin can create a dashboard.
    /// </summary>
    [Fact]
    public async Task AsAdmin_CreateDashboard_ShouldPass()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        var command = new CreateDashboardCommand
        {
            Name = "Sales Dashboard",
            Description = "Dashboard for tracking sales metrics"
        };

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/dashboards", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var result = await response.Content.ReadAsJsonAsync<CreateDashboardCommandResponse>();
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(0);
    }

    /// <summary>
    /// Ensure an admin can create a dashboard with only name.
    /// </summary>
    [Fact]
    public async Task AsAdmin_CreateDashboardOnlyName_ShouldPass()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        var command = new CreateDashboardCommand
        {
            Name = "Simple Dashboard"
        };

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/dashboards", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var result = await response.Content.ReadAsJsonAsync<CreateDashboardCommandResponse>();
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(0);
    }

    /// <summary>
    /// Ensure a user does not have access to create a dashboard.
    /// </summary>
    [Fact]
    public async Task AsUser_ShouldFail()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.User5);
        var command = new CreateDashboardCommand
        {
            Name = "User Dashboard",
            Description = "Should not be allowed"
        };

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/dashboards", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Ensure that dashboard name is required.
    /// </summary>
    [Fact]
    public async Task Name_Required_ShouldFail()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/dashboards", new { });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problems = await response.Content.ReadAsJsonAsync<ValidationProblemDetails>();
        problems.ShouldNotBeNull();
        problems.Errors.ShouldContain(p => p.Key.Contains("name") &&
            p.Value.Contains("'name' must not be empty."));
    }

    /// <summary>
    /// Ensure dashboard name length is validated.
    /// </summary>
    [Fact]
    public async Task Name_TooLong_ShouldFail()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        var command = new CreateDashboardCommand
        {
            Name = new string('A', 101), // 101 characters, exceeds max of 100
            Description = "Valid description"
        };

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/dashboards", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problems = await response.Content.ReadAsJsonAsync<ValidationProblemDetails>();
        problems.ShouldNotBeNull();
        problems.Errors.ShouldContain(p => p.Key.Contains("name"));
        var nameErrors = problems.Errors.First(p => p.Key.Contains("name"));
        nameErrors.Value.ShouldContain("The length of 'name' must be 100 characters or fewer. You entered 101 characters.");
    }

    /// <summary>
    /// Ensure description length is validated.
    /// </summary>
    [Fact]
    public async Task Description_TooLong_ShouldFail()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        var command = new CreateDashboardCommand
        {
            Name = "Valid Dashboard",
            Description = new string('A', 501) // 501 characters, exceeds max of 500
        };

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/dashboards", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problems = await response.Content.ReadAsJsonAsync<ValidationProblemDetails>();
        problems.ShouldNotBeNull();
        problems.Errors.ShouldContain(p => p.Key.Contains("description"));
        var descriptionErrors = problems.Errors.First(p => p.Key.Contains("description"));
        descriptionErrors.Value.ShouldContain("The length of 'description' must be 500 characters or fewer. You entered 501 characters.");
    }

    /// <summary>
    /// Ensure request fails if dashboard name already exists.
    /// </summary>
    [Fact]
    public async Task DuplicateName_ShouldFail()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        
        // Create first dashboard
        var firstCommand = new CreateDashboardCommand
        {
            Name = "Analytics Dashboard",
            Description = "First dashboard"
        };
        await session.Api.PostAsJsonAsync("/api/dashboards", firstCommand);

        // Try to create duplicate
        var duplicateCommand = new CreateDashboardCommand
        {
            Name = "Analytics Dashboard", // Same name
            Description = "Duplicate dashboard"
        };

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/dashboards", duplicateCommand);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Now that validation is in the validator, it should return ValidationProblemDetails
        var problems = await response.Content.ReadAsJsonAsync<ValidationProblemDetails>();
        problems.ShouldNotBeNull();
        problems.Errors.ShouldContain(p => p.Key.Contains("name"));
        var nameErrors = problems.Errors.First(p => p.Key.Contains("name"));
        nameErrors.Value.ShouldContain("Dashboard name already exists.");
    }

    /// <summary>
    /// Ensure various valid dashboard names work.
    /// </summary>
    [Theory]
    [InlineData("Sales Dashboard")]
    [InlineData("Marketing-Analytics")]
    [InlineData("HR_Dashboard_2024")]
    [InlineData("Dashboard 123")]
    [InlineData("A")]
    public async Task ValidDashboardNames_ShouldPass(string dashboardName)
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        var command = new CreateDashboardCommand
        {
            Name = dashboardName,
            Description = "Valid dashboard description"
        };

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/dashboards", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var result = await response.Content.ReadAsJsonAsync<CreateDashboardCommandResponse>();
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(0);
    }

    /// <summary>
    /// Ensure created dashboard is persisted in database.
    /// </summary>
    [Fact]
    public async Task CreateDashboard_ShouldPersistInDatabase()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        var command = new CreateDashboardCommand
        {
            Name = "Test Persistence Dashboard",
            Description = "Testing database persistence"
        };

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/dashboards", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var result = await response.Content.ReadAsJsonAsync<CreateDashboardCommandResponse>();
        result.ShouldNotBeNull();

        // Verify in database
        var dashboard = await session.Repository.Dashboards
            .FirstOrDefaultAsync(d => d.Id == result.Id);
        
        dashboard.ShouldNotBeNull();
        dashboard.Name.ShouldBe(command.Name);
        dashboard.Description.ShouldBe(command.Description);
        dashboard.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        dashboard.DeletedAt.ShouldBeNull();
    }

    /// <summary>
    /// Ensure response contains correct created resource location.
    /// </summary>
    [Fact]
    public async Task CreateDashboard_ShouldReturnCorrectLocation()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        var command = new CreateDashboardCommand
        {
            Name = "Location Test Dashboard",
            Description = "Testing location header"
        };

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/dashboards", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        
        var result = await response.Content.ReadAsJsonAsync<CreateDashboardCommandResponse>();
        result.ShouldNotBeNull();
        
        response.Headers.Location?.ToString().ShouldBe($"/api/dashboards/{result.Id}");
    }
}
