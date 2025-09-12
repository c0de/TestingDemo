namespace TestingDemo.Entities.Models;

public class Dashboard
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<UserDashboard> Users { get; set; }
}
