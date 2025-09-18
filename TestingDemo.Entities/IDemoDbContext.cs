// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using TestingDemo.Entities.Models;

namespace TestingDemo.Entities;

/// <summary>
/// Database context interface for the demo application.
/// </summary>
public interface IDemoDbContext : IDisposable
{
    // Tables
    DbSet<User> Users { get; set; }
    DbSet<Dashboard> Dashboards { get; set; }

    // Views
    DbSet<ActiveUsersView> ActiveUsersView { get; set; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
