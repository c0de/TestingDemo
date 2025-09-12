using Microsoft.EntityFrameworkCore;
using TestingDemo.Entities.Models;

namespace TestingDemo.Entities;

/// <summary>
/// Database context interface for the demo application.
/// </summary>
public interface IDemoDbContext: IDisposable
{
    DbSet<User> Users { get; set; }
    DbSet<Dashboard> Dashboards { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
