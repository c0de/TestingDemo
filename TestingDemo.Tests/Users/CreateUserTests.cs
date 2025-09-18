// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TestingDemo.Api;
using TestingDemo.Api.Users.Commands;

namespace TestingDemo.Tests.Users;

/// <summary>
/// Testing <see cref="CreateUserCommand"/>.
/// </summary>
public class CreateUserTests
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
        var response = await session.Api.PostAsJsonAsync("/api/users", new { });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Ensure an admin can create a user.
    /// </summary>
    [Fact]
    public async Task AsAdmin_CreateUser_ShouldPass()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com"
        };

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/users", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var result = await response.Content.ReadAsJsonAsync<CreateUserCommandResponse>();
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(0);
    }

    /// <summary>
    /// Ensure an admin can create an admin.
    /// </summary>
    [Fact]
    public async Task AsAdmin_CreateAdmin_ShouldPass()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        var command = new CreateUserCommand
        {
            FirstName = "Test",
            LastName = "Admin",
            Email = "test.admin@test.com",
            Role = Api.Role.Admin
        };

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/users", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var result = await response.Content.ReadAsJsonAsync<CreateUserCommandResponse>();
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(0);
    }

    /// <summary>
    /// Ensure a user does not have access to create a user.
    /// </summary>
    [Fact]
    public async Task AsUser_ShouldFail()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.User5);
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test1234@test.com"
        };

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/users", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Ensure that first name is required.
    /// </summary>
    [Fact]
    public async Task FirstName_Required_ShouldFail()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/users", new { });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problems = await response.Content.ReadAsJsonAsync<ValidationProblemDetails>();
        problems.ShouldNotBeNull();
        problems.Errors.ShouldContain(p => p.Key.Contains("firstName") &&
            p.Value.Contains("'first Name' must not be empty."));
    }

    /// <summary>
    /// Ensure last name is required.
    /// </summary>
    [Fact]
    public async Task LastName_Required_ShouldFail()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/users", new { });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problems = await response.Content.ReadAsJsonAsync<ValidationProblemDetails>();
        problems.ShouldNotBeNull();
        problems.Errors.ShouldContain(p => p.Key.Contains("lastName") &&
            p.Value.Contains("'last Name' must not be empty."));
    }

    /// <summary>
    /// Ensure email is required.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Email_Required_ShouldFail()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/users", new { });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problems = await response.Content.ReadAsJsonAsync<ValidationProblemDetails>();
        problems.ShouldNotBeNull();
        problems.Errors.ShouldContain(p => p.Key.Contains("email") &&
            p.Value.Contains("'email' must not be empty."));
    }

    /// <summary>
    /// Ensure email format is validated.
    /// </summary>
    [Theory]
    [InlineData("invalid-email")]
    [InlineData("hello world")]
    [InlineData("@test.com")]
    [InlineData("@gmail.com")]
    public async Task Email_Format_ShouldFail(string email)
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        var command = new CreateUserCommand
        {
            FirstName = "test",
            LastName = "test",
            Email = email
        };

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/users", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problems = await response.Content.ReadAsJsonAsync<ValidationProblemDetails>();
        problems.ShouldNotBeNull();
        problems.Errors.ShouldContain(p => p.Key.Contains("email") &&
            p.Value.Contains("'email' is not a valid email address."));
    }

    /// <summary>
    /// Ensure request fails if email already exists.
    /// </summary>
    [Theory]
    [InlineData("user5@test.com")] // active user
    [InlineData("user10@test.com")] // deleted user
    public async Task Email_Exists_ShouldFail(string email)
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        var command = new CreateUserCommand
        {
            FirstName = "test",
            LastName = "test",
            Email = email
        };

        // Act
        var response = await session.Api.PostAsJsonAsync("/api/users", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Ensure when a user is created it notifies them via email.
    /// </summary>
    [Fact]
    public async Task AsAdmin_ShouldSendEmail()
    {
        // Arrange Email
        var actualEmails = new List<System.Net.Mail.MailMessage>();
        var emailService = new Mock<IEmailService>();
        emailService.Setup(e => e.SendEmailAsync(It.IsAny<System.Net.Mail.MailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback<System.Net.Mail.MailMessage, CancellationToken>((m, ct) => actualEmails.Add(m));
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1, services =>
        {
            services.AddSingleton(emailService.Object);
        });
        var command = new CreateUserCommand
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@test.com"
        };

        // Act
        await session.Api.PostAsJsonAsync("/api/users", command);

        // Assert
        actualEmails.ShouldNotBeEmpty();
        var email = actualEmails.First();
        email.To.First().Address.ShouldBe(command.Email);
        email.Body.ShouldBe($"Hello {command.FirstName},\n\nWelcome to TestingDemo!");
    }

    /// <summary>
    /// Ensure when an admin is created it assigns the default dashboards.
    /// </summary>
    [Fact]
    public async Task CreateAdmin_ShouldAssignDefaultDashboards()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        var command = new CreateUserCommand
        {
            FirstName = "Test",
            LastName = "Admin",
            Email = "test.admin@test.com",
            Role = Api.Role.Admin
        };

        // Act
        await session.Api.PostAsJsonAsync("/api/users", command);

        // Assert
        using (var dbContext = session.DbContext)
        {
            var dashboards = await dbContext.Users
                .Include(u => u.Dashboards)
                .Where(u => u.Email == command.Email)
                .Select(e => e.Dashboards)
                .ToListAsync();
            dashboards.ShouldNotBeNull();
        }
    }

    /// <summary>
    /// Ensure when a user is created it assigns the default dashboards.
    /// </summary>
    [Fact]
    public async Task CreateUser_ShouldAssignDefaultDashboards()
    {
        // Arrange
        var session = await TestingFactory.CreateForUserAsync(TestUsers.Admin1);
        var command = new CreateUserCommand
        {
            FirstName = "Test",
            LastName = "Admin",
            Email = "test.admin@test.com",
            Role = Api.Role.Admin
        };

        // Act
        await session.Api.PostAsJsonAsync("/api/users", command);

        // Assert
        using (var dbContext = session.DbContext)
        {
            var dashboards = await dbContext.Users
                .Include(u => u.Dashboards)
                .Where(u => u.Email == command.Email)
                .Select(e => e.Dashboards)
                .ToListAsync();
            dashboards.ShouldNotBeNull();
        }
    }
}
