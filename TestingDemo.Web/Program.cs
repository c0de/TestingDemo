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
    options.UseSqlServer(connectionString, e =>
    {
        e.EnableRetryOnFailure();
        e.CommandTimeout(60); // 60 seconds
        e.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
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
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "TestIssuer",
        ValidAudience = "TestAudience",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("Super-Secret-Testing-Demo-Secret"))
    };
});


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
