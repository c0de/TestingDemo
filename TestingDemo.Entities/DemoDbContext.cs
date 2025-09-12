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
}
