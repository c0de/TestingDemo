// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestingDemo.Entities;

namespace TestingDemo.Tests;

/// <summary>
/// Factory for testing the Testing Demo application.
/// </summary>
public class TestingDemoWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Action<IServiceCollection>? _serviceAction;
    private readonly string _env;

    /// <summary>
    /// Create an instance of the factory.
    /// </summary>
    /// <param name="serviceAction"></param>
    /// <param name="env"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public TestingDemoWebApplicationFactory(Action<IServiceCollection>? serviceAction = null,
        string? env = null)
    {
        _serviceAction = serviceAction;
        _env = env ?? "Testing";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_env);
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DemoDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Remove the existing IDemoDbContext registration
            var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDemoDbContext));
            if (contextDescriptor != null)
            {
                services.Remove(contextDescriptor);
            }

            // Remove the existing DemoDbContext registration
            var demoContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DemoDbContext));
            if (demoContextDescriptor != null)
            {
                services.Remove(demoContextDescriptor);
            }

            // Add SQL Server DbContext for testing
            var connectionString = "Server=localhost; Integrated Security=True; Encrypt=True; TrustServerCertificate=True; Database=TestDatabase;";
            services.AddDbContext<DemoDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure();
                    sqlOptions.CommandTimeout(60);
                    sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
                // Enable detailed errors for testing
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            });

            // Register the interface
            services.AddScoped<IDemoDbContext>(provider => provider.GetRequiredService<DemoDbContext>());

            // invoke overrides
            _serviceAction?.Invoke(services);
        });
    }
}
