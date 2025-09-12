using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TestingDemo.Entities;
using TestingDemo.Entities.Models;

namespace TestingDemo.Tests;

/// <summary>
/// Model for testing an api endpoint.
/// </summary>
public class TestingInstance
{
    /// <summary>
    /// Injected IRepository.
    /// </summary>
    public required IRepository Repository { get; set; }
    /// <summary>
    /// Http Client to test.
    /// </summary>
    public required HttpClient Api { get; set; }
    /// <summary>
    /// Collection of Injected Services.
    /// </summary>
    public required IServiceProvider Services { get; set; }
    /// <summary>
    /// Authenticated User.
    /// </summary>
    public User? User { get; set; }
}

/// <summary>
/// Factory for creating testing instances.
/// </summary>
public static class TestingFactory
{
    /// <summary>
    /// Create an anonymous instance.
    /// </summary>
    /// <returns></returns>
    public static async Task<TestingInstance> CreateAnonymousAsync()
    {
        var dbContext = new DemoDbContext(new DbContextOptionsBuilder<DemoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseAsyncSeeding(SeedDatabaseAsync)
            .Options);
        await dbContext.Database.EnsureCreatedAsync();

        var webFactory = new TestingDemoWebApplicationFactory(dbContext);

        return new TestingInstance
        {
            Repository = dbContext,
            Api = webFactory.CreateClient(),
            Services = webFactory.Services,
        };
    }

    /// <summary>
    /// Database Seeding.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="arg2"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private static async Task SeedDatabaseAsync(DbContext context, bool arg2, CancellationToken token)
    {
        foreach (var user in TestUsers.All)
        {
            context.Add(user);
        }
        await context.SaveChangesAsync(token);
    }

    /// <summary>
    /// Create an instance for a user.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="action">service collection action</param>
    /// <param name="env">environment to create</param>
    /// <returns></returns>
    public static async Task<TestingInstance> CreateForUserAsync(User user,
        Action<IServiceCollection> action = null,
        string env = null)
    {
        var dbContext = new DemoDbContext(new DbContextOptionsBuilder<DemoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseAsyncSeeding(SeedDatabaseAsync)
            .Options);
        await dbContext.Database.EnsureCreatedAsync();

        var webFactory = new TestingDemoWebApplicationFactory(dbContext, action, env);
        var httpClient = webFactory.CreateClient();
        var token = GenerateJwtToken(user);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return new TestingInstance
        {
            Repository = dbContext,
            Api = httpClient,
            Services = webFactory.Services,
            User = user,
        };
    }

    /// <summary>
    /// Generate a Testing JWT token for a user.
    /// </summary>
    /// <param name="user">user</param>
    /// <param name="secret">secret key</param>
    /// <returns></returns>
    private static string GenerateJwtToken(User user, string secret = "Super-Secret-Testing-Demo-Secret")
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
