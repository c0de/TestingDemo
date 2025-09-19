// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using TestingDemo.Api;
using TestingDemo.Api.Services;
using TestingDemo.Entities;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment.EnvironmentName;

// Add DbContext
builder.Services.AddDbContext<DemoDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(60); // 60 seconds
        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        //options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); // For read-only scenarios
        options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
        options.EnableDetailedErrors(builder.Environment.IsDevelopment());
    });
});
builder.Services.AddScoped<IDemoDbContext>(e => e.GetRequiredService<DemoDbContext>());

// Add services to the container.
builder.Services.AddFastEndpoints();
builder.Services.TryAddScoped<IEmailService, EmailService>();
builder.Services.TryAddScoped<IAssetService, AssetService>();
builder.Services.TryAddScoped<ITemplateService, TemplateService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT secret key not configured");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole(Role.Admin.ToString()))
    .AddPolicy("AllUsers", policy => policy.RequireRole(RoleExtensions.GetAllRoleNames()));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseFastEndpoints();

app.Run();

/// <summary>
/// Partial program is required for testing purposes.
/// </summary>
public partial class Program { }
