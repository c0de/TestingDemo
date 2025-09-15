// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestingDemo.Entities;

namespace TestingDemo.Entities.Migrations;

/// <summary>
/// Extension methods for configuring DemoDbContext with migrations.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds DemoDbContext to the service collection configured for SQL Server with migrations in this assembly.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">The database connection string</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddDemoDbContextWithMigrations(
        this IServiceCollection services, 
        string connectionString)
    {
        return services.AddDbContext<DemoDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ServiceCollectionExtensions).Assembly.GetName().Name);
            });
        });
    }

    /// <summary>
    /// Adds DemoDbContext to the service collection with custom configuration and migrations in this assembly.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">The database connection string</param>
    /// <param name="configureOptions">Additional configuration options</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddDemoDbContextWithMigrations(
        this IServiceCollection services,
        string connectionString,
        Action<DbContextOptionsBuilder> configureOptions)
    {
        return services.AddDbContext<DemoDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ServiceCollectionExtensions).Assembly.GetName().Name);
            });
            
            configureOptions?.Invoke(options);
        });
    }
}
