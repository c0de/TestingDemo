// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TestingDemo.Entities;

/// <summary>
/// Factory for creating instances of DemoDbContext at design time.
/// </summary>
public class DemoDbContextFactory : IDesignTimeDbContextFactory<DemoDbContext>
{
    /// <summary>
    /// Creates a new instance of DemoDbContext with the specified options.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public DemoDbContext CreateDbContext(string[] args)
    {
        var connectionString = "Server=localhost; Integrated Security=True; Encrypt=True; TrustServerCertificate=True; Database=TestDatabase;";
        var optionsBuilder = new DbContextOptionsBuilder<DemoDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new DemoDbContext(optionsBuilder.Options);
    }
}
