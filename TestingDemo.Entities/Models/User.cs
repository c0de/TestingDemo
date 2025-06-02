namespace TestingDemo.Entities.Models;

public class User
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    public virtual ICollection<UserDashboard> Dashboards { get; set; }
}
