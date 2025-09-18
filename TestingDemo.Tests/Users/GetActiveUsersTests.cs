// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.Http.Json;
using TestingDemo.Api.Users.Queries;

namespace TestingDemo.Tests.Users;

/// <summary>
/// Tests for <see cref="GetActiveUsersQuery"/>
/// </summary>
/// <remarks>
/// Showing that we can execute results of a view.
/// </remarks>
public class GetActiveUsersTests
{
    /// <summary>
    /// Ensure an anynomous is unauthorized.
    /// </summary>
    [Fact]
    public async Task Anynomous_ShouldFail()
    {
        // Arrange
        using var session = await TestingFactory.CreateAnonymousAsync();

        // Act
        var response = await session.Api.GetAsync("/api/activeusers");

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
        using var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);

        // Act
        var response = await session.Api.GetAsync("/api/activeusers");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserResponse>>();
        users.ShouldNotBeNull();
        users.Count().ShouldBeGreaterThan(0);

        // inactive user should not be in the list
        users.ShouldNotContain(e => e.Id == TestUsers.InactiveAdmin2.Id);
        users.ShouldNotContain(e => e.Id == TestUsers.InactiveUser10.Id);
    }

    /// <summary>
    /// Ensure user can get a list of all users.
    /// </summary>
    [Fact]
    public async Task AsUser_ShouldPass()
    {
        // Arrange
        using var session = await TestingFactory.CreateForUserAsync(TestUsers.User6);

        // Act
        var response = await session.Api.GetAsync("/api/activeusers");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserResponse>>();
        users.ShouldNotBeNull();
        users.Count().ShouldBeGreaterThan(0);

        // admin should be in the list
        users.ShouldContain(e => e.Id == TestUsers.Admin1.Id);

        // inactive user should not be in the list
        users.ShouldNotContain(e => e.Id == TestUsers.InactiveAdmin2.Id);
        users.ShouldNotContain(e => e.Id == TestUsers.InactiveUser10.Id);
    }
}
