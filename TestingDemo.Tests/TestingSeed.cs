// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using TestingDemo.Entities;
using TestingDemo.Entities.Models;

namespace TestingDemo.Tests;

/// <summary>
/// Seeds the database with default data for testing purposes.
/// </summary>
public static class TestingSeed
{
    /// <summary>
    /// Seed Database with default test data.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="arg2"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task SeedDatabaseAsync(DemoDbContext dbContext, bool arg2, CancellationToken cancellationToken)
    {
        await SeedUsersAsync(dbContext, cancellationToken);
        await SeedDashboardsAsync(dbContext, cancellationToken);
    }

    /// <summary>
    /// Seeds the database with test users.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="token">Cancellation token.</param>
    private static async Task SeedUsersAsync(DemoDbContext dbContext, CancellationToken token)
    {
        await dbContext.SetIdentityInsertOn("Users", token);

        try
        {
            foreach (var user in TestUsers.All)
            {
                dbContext.Users.Add(user);
            }
            await dbContext.SaveChangesAsync(token);
        }
        finally
        {
            await dbContext.SetIdentityInsertOff("Users", token);
        }
    }

    /// <summary>
    /// Seeds the database with test dashboards.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="token">Cancellation token.</param>
    private static async Task SeedDashboardsAsync(DemoDbContext dbContext, CancellationToken token)
    {
        await dbContext.SetIdentityInsertOn("Dashboards", token);

        try
        {
            dbContext.Dashboards.AddRange([
                new Dashboard
                {
                    Id = 1,
                    Name = "Admin Dashboard",
                    CreatedAt = DateTime.UtcNow,
                },
                new Dashboard
                {
                    Id = 2,
                    Name = "User Dashboard",
                    CreatedAt = DateTime.UtcNow,
                }
            ]);
            await dbContext.SaveChangesAsync(token);
        }
        finally
        {
            await dbContext.SetIdentityInsertOff("Dashboards", token);
        }
    }

    /// <summary>
    /// Set Identity insert ON for the specified table if using SQL Server.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="tableName"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private static async Task SetIdentityInsertOn(this DemoDbContext context, string tableName, CancellationToken token)
    {
        var isSqlServer = context.Database.IsSqlServer();
        if (isSqlServer)
        {
            var command = $"SET IDENTITY_INSERT [{tableName}] ON";
            await context.Database.ExecuteSqlRawAsync(command, cancellationToken: token);
        }
    }

    /// <summary>
    /// Set Identity insert OFF for the specified table if using SQL Server.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="tableName"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private static async Task SetIdentityInsertOff(this DemoDbContext context, string tableName, CancellationToken token)
    {
        var isSqlServer = context.Database.IsSqlServer();
        if (isSqlServer)
        {
            var command = $"SET IDENTITY_INSERT [{tableName}] OFF";
            await context.Database.ExecuteSqlRawAsync(command, cancellationToken: token);
        }
    }
}
