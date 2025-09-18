// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.Http.Json;

namespace TestingDemo.Tests.Users;

public class ProcessUsersTests
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
        var response = await session.Api.PostAsJsonAsync("/api/users/process", new { });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Ensure an admin can execute process users.
    /// </summary>
    [Fact]
    public async Task AsAdmin_ShouldPass()
    {
        // Arrange
        using var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/users/process", new { });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
