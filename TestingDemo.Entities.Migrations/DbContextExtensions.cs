// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TestingDemo.Entities.Migrations;

/// <summary>
/// Extension methods for DbContext.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Synchronizes SQL objects (stored procedures, functions, and views) from embedded resources with the database.
    /// </summary>
    /// <param name="dbContext">The database context</param>
    /// <param name="serviceProvider">Service provider for dependency injection (optional)</param>
    /// <param name="assembly">Assembly containing embedded resources (optional, defaults to DbContext assembly)</param>
    /// <param name="resourceNamespace">Namespace prefix for embedded resources (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Combined sync result for all SQL objects</returns>
    public static async Task<SyncResult> SyncSqlObjectsAsync(
        this DbContext dbContext,
        IServiceProvider serviceProvider = null,
        Assembly assembly = null,
        string resourceNamespace = null,
        CancellationToken cancellationToken = default)
    {
        SyncService syncService;

        if (serviceProvider != null)
        {
            // Try to get the service from DI container
            syncService = serviceProvider.GetService<SyncService>();

            if (syncService == null)
            {
                // Fallback: create with logger from DI if available
                var logger = serviceProvider.GetService<ILogger<SyncService>>()
                    ?? serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<SyncService>();

                syncService = new SyncService(
                    logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<SyncService>.Instance);
            }
        }
        else
        {
            // Create with null logger
            syncService = new SyncService(
                Microsoft.Extensions.Logging.Abstractions.NullLogger<SyncService>.Instance);
        }

        return await syncService.SyncSqlObjectsAsync(dbContext, assembly, resourceNamespace, cancellationToken);
    }
}
