// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using TestingDemo.Entities;
using TestingDemo.Entities.Models;

namespace TestingDemo.Tests;

public static class TestingSeed
{
    /// <summary>
    /// Seed Database with default test data.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="arg2"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task SeedDatabaseAsync(DemoDbContext context, bool arg2, CancellationToken token)
    {
        // seed users
        foreach (var user in TestUsers.All)
        {
            context.Users.Add(user);
        }
        await context.SaveChangesAsync(token);
        // seed dashboards
        context.Dashboards.AddRange([
            new Dashboard
            {
                Id = 1,
                Name = "Admin Dashboard"
            },
            new Dashboard
            {
                Id = 2,
                Name = "User Dashboard"
            }
        ]);
        await context.SaveChangesAsync(token);
    }
}
