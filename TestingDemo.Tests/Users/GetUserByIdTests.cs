// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.Http.Json;
using TestingDemo.Api.Users.Queries;

namespace TestingDemo.Tests.Users;

/// <summary>
/// Tests for <see cref="GetUserByIdQuery"/>
/// </summary>
public class GetUserByIdTests
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
        var response = await session.Api.GetAsync("/api/users/1");

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
        var response = await session.Api.GetAsync("/api/users/1");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        user.ShouldNotBeNull();
        user.Id.ShouldBe(1);
    }

    /// <summary>
    /// Ensure a user can get a lits of all users.
    /// </summary>
    [Fact]
    public async Task AsUser_ShouldPass()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.User5);

        // Act
        var response = await session.Api.GetAsync("/api/users/1");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        user.ShouldNotBeNull();
        user.Id.ShouldBe(1);
    }

    /// <summary>
    /// Ensure a user that does not exist returns not found.
    /// </summary>
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public async Task AsUser_NotFound_ShouldPass(int id)
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.User5);

        // Act
        var response = await session.Api.GetAsync($"/api/users/{id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Inactive user should return not found
    /// </summary>
    [Fact]
    public async Task AsUser_Inactive_ShouldPass()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.User5);

        // Act
        var response = await session.Api.GetAsync($"/api/users/{TestUsers.InactiveUser10.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
