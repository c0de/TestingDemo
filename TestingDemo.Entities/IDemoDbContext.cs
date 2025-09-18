// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
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
    /// Executes the given SQL against the database and returns the number of rows affected.
    /// </summary>
    /// <param name="sql">The SQL to execute.</param>
    /// <param name="timeoutInSeconds">Command timeout in seconds.</param>
    /// <param name="parameters">Optional SQL parameters.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> ExecuteAsync(string sql,
        int timeoutInSeconds,
        SqlParameter[] parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
