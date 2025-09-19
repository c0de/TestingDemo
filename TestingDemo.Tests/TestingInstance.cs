// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using TestingDemo.Entities;
using TestingDemo.Entities.Models;

namespace TestingDemo.Tests;

/// <summary>
/// Model for testing an api endpoint.
/// </summary>
public class TestingInstance : IDisposable
{
    /// <summary>
    /// Authenticated User.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Web Application Factory for testing.
    /// </summary>
    public required TestingDemoWebApplicationFactory WebApplicationFactory { get; set; }

    /// <summary>
    /// Get a fresh DbContext instance from the service scope.
    /// This ensures each database operation gets a fresh context.
    /// </summary>
    public IDemoDbContext DbContext
    {
        get
        {
            var scope = WebApplicationFactory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IDemoDbContext>();
            return dbContext;
        }
    }

    /// <summary>
    /// Get an HttpClient instance for testing the api.
    /// </summary>
    public HttpClient Api
    {
        get
        {
            // create http client from factory
            var httpClient = WebApplicationFactory.CreateClient();

            if (User != null)
            {
                // create jwt token and set auth header for the current user
                var token = this.GenerateJwtToken();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return httpClient;
        }
    }

    /// <summary>
    /// Dispose the instance.
    /// </summary>
    public void Dispose()
    {
        Api.Dispose();
        WebApplicationFactory.Dispose();
        GC.SuppressFinalize(this);
    }
}
