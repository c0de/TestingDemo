// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using TestingDemo.Entities.Models;

namespace TestingDemo.Entities;

/// <summary>
/// Database context for the demo application.
/// </summary>
public class DemoDbContext : DbContext, IDemoDbContext
{
    /// <summary>
    /// Constructor for DemoDbContext.
    /// </summary>
    /// <param name="options"></param>
    public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Dashboard> Dashboards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DemoDbContext).Assembly);
    }

    /// <summary>
    /// Executes the given SQL against the database and returns the number of rows affected.
    /// </summary>
    /// <param name="sql">The SQL to execute.</param>
    /// <param name="timeoutInSeconds">Command timeout in seconds.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public async Task<int> ExecuteAsync(string sql, int timeoutInSeconds, CancellationToken cancellationToken = default)
    {
        if (!Database.IsSqlServer())
        {
            throw new NotSupportedException("ExecuteAsync is only supported for SQL Server databases.");
        }

        Database.SetCommandTimeout(timeoutInSeconds);

        return await Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
