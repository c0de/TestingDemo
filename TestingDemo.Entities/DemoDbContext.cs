using Microsoft.EntityFrameworkCore;
using TestingDemo.Entities.Models;

namespace TestingDemo.Entities;

public class DemoDbContext : DbContext, IRepository
{
    public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DemoDbContext).Assembly);
    }
}
