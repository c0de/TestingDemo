// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TestingDemo.Entities;

namespace TestingDemo.Tests;

/// <summary>
/// Factory for testing the Testing Demo application.
/// </summary>
public class TestingDemoWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly DemoDbContext _dbContext;
    private readonly Action<IServiceCollection>? _serviceAction;
    private readonly string _env;

    /// <summary>
    /// Create an instance of the factory.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="serviceAction"></param>
    /// <param name="env"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public TestingDemoWebApplicationFactory(DemoDbContext dbContext,
        Action<IServiceCollection>? serviceAction = null,
        string? env = null)
    {
        _dbContext = dbContext;
        _serviceAction = serviceAction;
        _env = env ?? "Testing";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_env);
        builder.ConfigureServices(services =>
        {
            if (_dbContext != null)
            {
                services.AddSingleton<IDemoDbContext>(_dbContext);
            }

            // invoke overrides
            _serviceAction?.Invoke(services);
        });
    }
}
