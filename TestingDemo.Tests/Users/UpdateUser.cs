// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.Http.Json;

namespace TestingDemo.Tests.Users;

public class UpdateUserTests
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
        var response = await session.Api.PutAsJsonAsync("/api/users", new { });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
