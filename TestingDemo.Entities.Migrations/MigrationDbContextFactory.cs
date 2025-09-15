// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TestingDemo.Entities;

namespace TestingDemo.Entities.Migrations;

/// <summary>
/// Factory for creating instances of DemoDbContext at design time for migrations.
/// This factory is located in the migrations project to ensure migrations are generated here.
/// </summary>
public class MigrationDbContextFactory : IDesignTimeDbContextFactory<DemoDbContext>
{
    /// <summary>
    /// Creates a new instance of DemoDbContext with the specified options for migrations.
    /// </summary>
    /// <param name="args">Command line arguments (can contain connection string)</param>
    /// <returns>Configured DemoDbContext instance</returns>
    public DemoDbContext CreateDbContext(string[] args)
    {
        // Default connection string for migrations
        var connectionString = "Server=localhost; Integrated Security=True; Encrypt=True; TrustServerCertificate=True; Database=TestDatabase;";
        
        // Allow override via command line arguments
        if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
        {
            connectionString = args[0];
        }

        var optionsBuilder = new DbContextOptionsBuilder<DemoDbContext>();
        optionsBuilder.UseSqlServer(connectionString, options =>
        {
            // Specify that migrations should be placed in this assembly
            options.MigrationsAssembly(typeof(MigrationDbContextFactory).Assembly.GetName().Name);
        });

        return new DemoDbContext(optionsBuilder.Options);
    }
}
