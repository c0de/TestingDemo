using System.Net;
using System.Net.Http.Json;
using TestingDemo.Api.Users.Queries;

namespace TestingDemo.Tests.Users;

/// <summary>
/// Tests for <see cref="GetUsersQuery"/>
/// </summary>
public class GetUsers
{
    /// <summary>
    /// Ensure an anynomous is unauthorized.
    /// </summary>
    [Fact]
    public async Task Anynomous_ShouldFail()
    {
        // Arrange
        var session = await TestingFactory.CreateAnonymousAsync();

        // Act
        var response = await session.Api.GetAsync("/api/users");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Ensure an admin can get a lits of all users.
    /// </summary>
    [Fact]
    public async Task AsAdmin_ShouldPass()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);

        // Act
        var response = await session.Api.GetAsync("/api/users");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserResponse>>();
        users.ShouldNotBeNull();
        users.Count().ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Ensure user can get a list of all users.
    /// </summary>
    [Fact]
    public async Task AsUser_ShouldPass()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.User6);

        // Act
        var response = await session.Api.GetAsync("/api/users");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserResponse>>();
        users.ShouldNotBeNull();
        users.Count().ShouldBeGreaterThan(0);
    }
}
