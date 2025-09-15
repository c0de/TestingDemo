// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TestingDemo.Entities;
using TestingDemo.Entities.Migrations;
using TestingDemo.Entities.Models;

namespace TestingDemo.Tests;

/// <summary>
/// Factory for creating testing instances.
/// </summary>
public static class TestingFactory
{
    private static bool _sqlServerDbInitialized = false;
    private static readonly SemaphoreSlim _initializationSemaphore = new(1, 1);

    /// <summary>
    /// Create an anonymous instance.
    /// </summary>
    /// <returns></returns>
    public static async Task<TestingInstance> CreateAnonymousAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = await CreateInMemoryDbContextAsync(cancellationToken);
        var webFactory = new TestingDemoWebApplicationFactory(dbContext);

        return new TestingInstance
        {
            Repository = dbContext,
            Api = webFactory.CreateClient(),
            Services = webFactory.Services,
        };
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
        string env = null,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await CreateSqlServerDbContextAsync(cancellationToken);

        var webFactory = new TestingDemoWebApplicationFactory(null, action, env);

        // create test http client
        var httpClient = webFactory.CreateClient();

        // create jwt token and set auth header
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
    /// Create a in-memory DbContext instance with seeded data.
    /// </summary>
    /// <returns></returns>
    private static async Task<DemoDbContext> CreateInMemoryDbContextAsync(CancellationToken cancellationToken)
    {
        var options = new DbContextOptionsBuilder<DemoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseAsyncSeeding(TestingSeed.SeedDatabaseAsync)
            .Options;
        var dbContext = new DemoDbContext(options);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        return dbContext;
    }

    /// <summary>
    /// Create a SQL Server DbContext instance with seeded data.
    /// </summary>
    /// <returns></returns>
    private static async Task<DemoDbContext> CreateSqlServerDbContextAsync(CancellationToken cancellationToken)
    {
        // localhost connection string for testing - using Integrated Security for local development
        var connectionString = "Server=localhost; Integrated Security=True; Encrypt=True; TrustServerCertificate=True; Database=TestDatabase;";
        var options = new DbContextOptionsBuilder<DemoDbContext>()
            .UseSqlServer(connectionString)
            .UseAsyncSeeding(TestingSeed.SeedDatabaseAsync)
            .Options;
        var dbContext = new DemoDbContext(options);

        await InitializeSqlDatabaseAsync(dbContext, cancellationToken);

        return dbContext;
    }

    /// <summary>
    /// Initialize Sql Database only once.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task InitializeSqlDatabaseAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        if (_sqlServerDbInitialized)
        {
            return;
        }

        await _initializationSemaphore.WaitAsync(cancellationToken);
        try
        {
            // drop the database so we can create and seed
            await dbContext.Database.EnsureDeletedAsync(cancellationToken);

            // recommended for simple setup
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);

            // ef-migrations
            //await dbContext.Database.MigrateAsync(cancellationToken);

            // sync stored procedures, views, functions, etc.
            var result = await dbContext.SyncSqlObjectsAsync(cancellationToken: cancellationToken);

            if (result.ErrorCount > 0)
            {
                throw new InvalidOperationException($"Stored procedure sync failed with {result.ErrorCount} errors");
            }

            _sqlServerDbInitialized = true;
        }
        finally
        {
            _initializationSemaphore.Release();
        }
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
