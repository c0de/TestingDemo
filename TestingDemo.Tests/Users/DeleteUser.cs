using System.Net;
using TestingDemo.Api.Users.Commands;

namespace TestingDemo.Tests.Users;

/// <summary>
/// Testing <see cref="DeleteUserCommand"/>.
/// </summary>
public class DeleteUser
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
        var response = await session.Api.DeleteAsync("/api/users/1");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Ensure an user does not have access.
    /// </summary>
    [Fact]
    public async Task AsUser_ShouldFail()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.User6);

        // Act
        var response = await session.Api.DeleteAsync($"/api/users/1");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Ensure an admin can delete a user.
    /// </summary>
    [Fact]
    public async Task AsAdmin_ShouldPass()
    {
        // Arrange
        var userId = TestUsers.User5.Id;
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);

        // Act
        var response = await session.Api.DeleteAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        session.Repository.Users
            .Any(x => x.Id == userId && x.DeletedAt == null)
            .ShouldBeFalse("User should be deleted");
    }
}
