using Microsoft.EntityFrameworkCore;
using TestingDemo.Entities.Models;

namespace TestingDemo.Entities;

public interface IRepository: IDisposable
{
    DbSet<User> Users { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
