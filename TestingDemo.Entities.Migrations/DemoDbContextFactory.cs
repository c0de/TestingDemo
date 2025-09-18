// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TestingDemo.Entities.Migrations;

/// <summary>
/// Factory for creating DemoDbContext instances at design time for migrations.
/// </summary>
public class DemoDbContextFactory : IDesignTimeDbContextFactory<DemoDbContext>
{
    /// <summary>
    /// Creates a new instance of DemoDbContext for design-time operations.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Configured DemoDbContext instance</returns>
    public DemoDbContext CreateDbContext(string[] args)
    {
        // Default connection string for migrations - can be overridden via command line or environment
        var connectionString = GetConnectionString(args);

        var optionsBuilder = new DbContextOptionsBuilder<DemoDbContext>();

        // Use the static method from DemoDbContext to configure SQL Server with migrations
        DemoDbContext.ConfigureForSqlServer(optionsBuilder, connectionString);

        return new DemoDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Gets the connection string from various sources (args, environment, or default).
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Database connection string</returns>
    private static string GetConnectionString(string[] args)
    {
        // Check command line arguments first
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals("--connection-string", StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        // Check environment variable
        var envConnectionString = Environment.GetEnvironmentVariable("MIGRATION_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(envConnectionString))
        {
            return envConnectionString;
        }

        // Default connection string for local development
        return "Server=localhost;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Database=TestingDemoDev;";
    }
}
