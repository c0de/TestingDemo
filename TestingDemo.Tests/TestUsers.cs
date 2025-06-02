using TestingDemo.Api;
using TestingDemo.Entities.Models;

namespace TestingDemo.Tests;

public static class TestUsers
{
    /// <summary>
    /// Collection of all test users
    /// </summary>
    public static List<User> All
    {
        get
        {
            var objects = typeof(TestUsers).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(e => e.FieldType == typeof(User))
                .Select(e => (User)e.GetValue(null))
                .ToList();
            return objects;
        }
    }

    public static readonly User Admin1 = new()
    {
        Id = 1,
        FirstName = "Test",
        LastName = "Admin1",
        Email = "admin1@test.com",
        Role = Role.Admin.ToString()
    };
    public static readonly User InactiveAdmin2 = new()
    {
        Id = 2,
        FirstName = "Test",
        LastName = "InactiveAdmin2",
        Email = "admin2@test.com",
        Role = Role.Admin.ToString(),
        DeletedAt = DateTime.UtcNow.AddDays(-10)
    };
    public static readonly User User5 = new()
    {
        Id = 5,
        FirstName = "Test",
        LastName = "User5",
        Email = "user5@test.com",
        Role = Role.User.ToString()
    };
    public static readonly User User6 = new()
    {
        Id = 6,
        FirstName = "Test",
        LastName = "User6",
        Email = "user6@test.com",
        Role = Role.User.ToString()
    };
    public static readonly User InactiveUser10 = new()
    {
        Id = 10,
        FirstName = "Test",
        LastName = "InactiveUser",
        Email = "user10@test.com",
        DeletedAt = DateTime.UtcNow.AddDays(-10),
        Role = Role.User.ToString()
    };
}
