// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    public static async Task<TestingInstance> CreateAnonymousAsync(
        Action<IServiceCollection>? action = null,
        string? env = null,
        CancellationToken cancellationToken = default)
    {
        var webFactory = new TestingDemoWebApplicationFactory(action, env);

        await InitializeDatabaseAsync(webFactory, cancellationToken);

        return new TestingInstance
        {
            WebApplicationFactory = webFactory,
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
        Action<IServiceCollection>? action = null,
        string? env = null,
        CancellationToken cancellationToken = default)
    {
        var webFactory = new TestingDemoWebApplicationFactory(action, env);

        await InitializeDatabaseAsync(webFactory, cancellationToken);

        return new TestingInstance
        {
            User = user,
            WebApplicationFactory = webFactory,
        };
    }

    /// <summary>
    /// Initialize Sql Database only once.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task InitializeDatabaseAsync(TestingDemoWebApplicationFactory webFactory,
        CancellationToken cancellationToken)
    {
        if (_sqlServerDbInitialized)
        {
            return;
        }

        await _initializationSemaphore.WaitAsync(cancellationToken);
        try
        {
            // CRITICAL: Double-check pattern - check again after acquiring the lock
            if (_sqlServerDbInitialized)
            {
                return;
            }

            // get connection string from webFactory
            var scope = webFactory.Services.CreateScope();
            var connectionString = scope.ServiceProvider.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");

            // create db context options for sql server
            var options = new DbContextOptionsBuilder<DemoDbContext>()
                .UseSqlServer(connectionString, options =>
                {
                    var assemblyName = typeof(DemoDbContextFactory).Assembly.GetName().Name;
                    options.MigrationsAssembly(assemblyName);
                })
                .UseAsyncSeeding(TestingSeed.SeedDatabaseAsync)
                .Options;
            using var dbContext = new DemoDbContext(options);

            // drop the database so we can create and seed
            await dbContext.Database.EnsureDeletedAsync(cancellationToken);

            // create using ef core
            //await dbContext.Database.EnsureCreatedAsync(cancellationToken);

            // ef-migrations
            await dbContext.Database.MigrateAsync(cancellationToken);

            // sync stored procedures, views, functions, etc.
            var result = await dbContext.SyncSqlObjectsAsync(cancellationToken: cancellationToken);
            if (result.ErrorCount > 0)
            {
                throw new InvalidOperationException($"Stored procedure sync failed with {result.ErrorCount} errors");
            }

            _sqlServerDbInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
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
    internal static string GenerateJwtToken(User user, string secret = "Super-Secret-Testing-Demo-Secret")
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
