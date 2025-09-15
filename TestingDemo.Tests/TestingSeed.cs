// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
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
        await SeedUsersAsync(context, token);
        await SeedDashboardsAsync(context, token);
    }

    /// <summary>
    /// Seeds the database with test users.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="token">Cancellation token.</param>
    private static async Task SeedUsersAsync(DemoDbContext context, CancellationToken token)
    {
        var isSqlServer = context.Database.IsSqlServer();
        
        if (isSqlServer)
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Users ON", cancellationToken: token);
        }
        
        try
        {
            foreach (var user in TestUsers.All)
            {
                context.Users.Add(user);
            }
            await context.SaveChangesAsync(token);
        }
        finally
        {
            if (isSqlServer)
            {
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Users OFF", cancellationToken: token);
            }
        }
    }

    /// <summary>
    /// Seeds the database with test dashboards.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="token">Cancellation token.</param>
    private static async Task SeedDashboardsAsync(DemoDbContext context, CancellationToken token)
    {
        var isSqlServer = context.Database.IsSqlServer();
        
        if (isSqlServer)
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Dashboards ON", cancellationToken: token);
        }
        
        try
        {
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
        finally
        {
            if (isSqlServer)
            {
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Dashboards OFF", cancellationToken: token);
            }
        }
    }
}
