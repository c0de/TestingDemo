using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using TestingDemo.Api.Dashboards.Commands;
using TestingDemo.Api.Dashboards.Queries;

namespace TestingDemo.Tests.Dashboards;

/// <summary>
/// Tests for <see cref="GetDashboardsQuery"/>
/// </summary>
public class GetDashboardsTests
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
        var response = await session.Api.GetAsync("/api/dashboards");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Ensure an admin can get a list of all dashboards.
    /// </summary>
    [Fact]
    public async Task AsAdmin_ShouldPass()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);

        // Act
        var response = await session.Api.GetAsync("/api/dashboards");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var dashboards = await response.Content.ReadFromJsonAsync<IEnumerable<DashboardResponse>>();
        dashboards.ShouldNotBeNull();
    }

    /// <summary>
    /// Ensure a user can get a list of all dashboards.
    /// </summary>
    [Fact]
    public async Task AsUser_ShouldPass()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.User5);

        // Act
        var response = await session.Api.GetAsync("/api/dashboards");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var dashboards = await response.Content.ReadFromJsonAsync<IEnumerable<DashboardResponse>>();
        dashboards.ShouldNotBeNull();
    }

    /// <summary>
    /// Ensure that deleted dashboards are not returned.
    /// </summary>
    [Fact]
    public async Task DeletedDashboards_ShouldNotBeReturned()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        
        // Create a dashboard
        var createCommand = new CreateDashboardCommand
        {
            Name = "Test Dashboard",
            Description = "This dashboard will be deleted"
        };
        var createResponse = await session.Api.PostAsJsonAsync("/api/dashboards", createCommand);
        var createdDashboard = await createResponse.Content.ReadFromJsonAsync<CreateDashboardCommandResponse>();
        
        // Soft delete the dashboard directly in the database
        var dashboard = await session.Repository.Dashboards.FirstAsync(d => d.Id == createdDashboard.Id);
        dashboard.DeletedAt = DateTime.UtcNow;
        await session.Repository.SaveChangesAsync();

        // Act
        var response = await session.Api.GetAsync("/api/dashboards");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var dashboards = await response.Content.ReadFromJsonAsync<IEnumerable<DashboardResponse>>();
        dashboards.ShouldNotBeNull();
        dashboards.ShouldNotContain(d => d.Id == createdDashboard.Id);
    }

    /// <summary>
    /// Ensure response format is correct with proper dashboard data.
    /// </summary>
    [Fact]
    public async Task ResponseFormat_ShouldBeCorrect()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        
        // Create a dashboard to ensure we have data
        var createCommand = new CreateDashboardCommand
        {
            Name = "Analytics Dashboard",
            Description = "Dashboard for analytics"
        };
        var createResponse = await session.Api.PostAsJsonAsync("/api/dashboards", createCommand);
        var createdDashboard = await createResponse.Content.ReadFromJsonAsync<CreateDashboardCommandResponse>();

        // Act
        var response = await session.Api.GetAsync("/api/dashboards");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var dashboards = await response.Content.ReadFromJsonAsync<IEnumerable<DashboardResponse>>();
        dashboards.ShouldNotBeNull();
        
        var dashboard = dashboards.FirstOrDefault(d => d.Id == createdDashboard.Id);
        dashboard.ShouldNotBeNull();
        dashboard.Id.ShouldBe(createdDashboard.Id);
        dashboard.Name.ShouldBe(createCommand.Name);
        dashboard.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    /// <summary>
    /// Ensure multiple dashboards are returned correctly.
    /// </summary>
    [Fact]
    public async Task MultipleDashboards_ShouldReturnAll()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        
        // Act
        var response = await session.Api.GetAsync("/api/dashboards");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var dashboards = await response.Content.ReadFromJsonAsync<IEnumerable<DashboardResponse>>();
        dashboards.ShouldNotBeNull();
        dashboards.Count().ShouldBeGreaterThanOrEqualTo(2);
        
        dashboards.ShouldContain(d => d.Name == "Admin Dashboard");
        dashboards.ShouldContain(d => d.Name == "User Dashboard");
    }
}
