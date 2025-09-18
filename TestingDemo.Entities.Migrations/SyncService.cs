// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TestingDemo.Entities.Migrations;

/// <summary>
/// Service for synchronizing stored procedures, functions, and views from embedded resources with the database.
/// </summary>
public class SyncService
{
    private readonly ILogger<SyncService> _logger;

    public SyncService(ILogger<SyncService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Synchronizes stored procedures, functions, and views from embedded resources with the database.
    /// </summary>
    /// <param name="dbContext">The Entity Framework DbContext</param>
    /// <param name="assembly">Assembly containing embedded resources (defaults to calling assembly)</param>
    /// <param name="resourceNamespace">Namespace prefix for embedded resources (e.g., "TestingDemo.Entities.StoredProcedures")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Combined sync result containing counts of operations performed for all object types</returns>
    /// <exception cref="ArgumentNullException">Thrown when dbContext is null</exception>
    /// <exception cref="NotSupportedException">Thrown when database provider is not SQL Server</exception>
    public async Task<DatabaseObjectSyncResult> SyncSqlObjectsAsync(
        DbContext dbContext,
        Assembly assembly = null,
        string resourceNamespace = null,
        CancellationToken cancellationToken = default)
    {
        if (dbContext == null)
            throw new ArgumentNullException(nameof(dbContext));

        if (!dbContext.Database.IsSqlServer())
            throw new NotSupportedException("Database object synchronization is only supported for SQL Server databases.");

        _logger.LogInformation("Starting database object synchronization (procedures, functions, views)");

        var combinedResult = new DatabaseObjectSyncResult();

        // Sync stored procedures
        var procedureResult = await SyncStoredProceduresAsync(dbContext, assembly, resourceNamespace, cancellationToken);
        combinedResult.Add(procedureResult);

        // Sync functions
        var functionResult = await SyncFunctionsAsync(dbContext, assembly, resourceNamespace, cancellationToken);
        combinedResult.Add(functionResult);

        // Sync views
        var viewResult = await SyncViewsAsync(dbContext, assembly, resourceNamespace, cancellationToken);
        combinedResult.Add(viewResult);

        _logger.LogInformation("Database object synchronization completed. Total - Created: {Created}, Altered: {Altered}, Dropped: {Dropped}, Errors: {Errors}",
            combinedResult.CreatedCount, combinedResult.AlteredCount, combinedResult.DroppedCount, combinedResult.ErrorCount);

        return combinedResult;
    }

    /// <summary>
    /// Synchronizes stored procedures from embedded resources with the database.
    /// Drops procedures that exist in the database but not in the embedded resources,
    /// and creates/alters procedures from embedded SQL resources.
    /// </summary>
    /// <param name="dbContext">The Entity Framework DbContext</param>
    /// <param name="assembly">Assembly containing embedded resources (defaults to calling assembly)</param>
    /// <param name="resourceNamespace">Namespace prefix for embedded resources (e.g., "TestingDemo.Entities.StoredProcedures")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sync result containing counts of operations performed</returns>
    /// <exception cref="ArgumentNullException">Thrown when dbContext is null</exception>
    /// <exception cref="NotSupportedException">Thrown when database provider is not SQL Server</exception>
    public async Task<StoredProcedureSyncResult> SyncStoredProceduresAsync(
        DbContext dbContext,
        Assembly assembly = null,
        string resourceNamespace = null,
        CancellationToken cancellationToken = default)
    {
        if (dbContext == null)
            throw new ArgumentNullException(nameof(dbContext));

        if (!dbContext.Database.IsSqlServer())
            throw new NotSupportedException("Stored procedure synchronization is only supported for SQL Server databases.");

        assembly ??= typeof(SyncService).Assembly;

        // Default namespace for embedded resources
        resourceNamespace ??= $"{assembly.GetName().Name}.StoredProcedures";

        _logger.LogInformation("Starting stored procedure synchronization from embedded resources in assembly: {Assembly}, namespace: {Namespace}",
            assembly.GetName().Name, resourceNamespace);

        return await SyncDatabaseObjectsAsync<StoredProcedureSyncResult>(
            dbContext, assembly, resourceNamespace, "stored procedure", "procedures",
            GetDatabaseStoredProceduresAsync, DropStoredProcedureAsync,
            @"(?:CREATE|ALTER)\s+PROC(?:EDURE)?\s+(?:\[?(?<schema>\w+)\]?\.)?\[?(?<name>\w+)\]?",
            cancellationToken);
    }

    /// <summary>
    /// Synchronizes functions from embedded resources with the database.
    /// Drops functions that exist in the database but not in the embedded resources,
    /// and creates/alters functions from embedded SQL resources.
    /// </summary>
    /// <param name="dbContext">The Entity Framework DbContext</param>
    /// <param name="assembly">Assembly containing embedded resources (defaults to calling assembly)</param>
    /// <param name="resourceNamespace">Namespace prefix for embedded resources (e.g., "TestingDemo.Entities.Functions")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sync result containing counts of operations performed</returns>
    /// <exception cref="ArgumentNullException">Thrown when dbContext is null</exception>
    /// <exception cref="NotSupportedException">Thrown when database provider is not SQL Server</exception>
    public async Task<FunctionSyncResult> SyncFunctionsAsync(
        DbContext dbContext,
        Assembly assembly = null,
        string resourceNamespace = null,
        CancellationToken cancellationToken = default)
    {
        if (dbContext == null)
            throw new ArgumentNullException(nameof(dbContext));

        if (!dbContext.Database.IsSqlServer())
            throw new NotSupportedException("Function synchronization is only supported for SQL Server databases.");

        assembly ??= typeof(SyncService).Assembly;

        // Default namespace for embedded resources
        resourceNamespace ??= $"{assembly.GetName().Name}.Functions";

        _logger.LogInformation("Starting function synchronization from embedded resources in assembly: {Assembly}, namespace: {Namespace}",
            assembly.GetName().Name, resourceNamespace);

        return await SyncDatabaseObjectsAsync<FunctionSyncResult>(
            dbContext, assembly, resourceNamespace, "function", "functions",
            GetDatabaseFunctionsAsync, DropFunctionAsync,
            @"(?:CREATE|ALTER)\s+FUNCTION\s+(?:\[?(?<schema>\w+)\]?\.)?\[?(?<name>\w+)\]?",
            cancellationToken);
    }

    /// <summary>
    /// Synchronizes views from embedded resources with the database.
    /// Drops views that exist in the database but not in the embedded resources,
    /// and creates/alters views from embedded SQL resources.
    /// </summary>
    /// <param name="dbContext">The Entity Framework DbContext</param>
    /// <param name="assembly">Assembly containing embedded resources (defaults to calling assembly)</param>
    /// <param name="resourceNamespace">Namespace prefix for embedded resources (e.g., "TestingDemo.Entities.Views")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sync result containing counts of operations performed</returns>
    /// <exception cref="ArgumentNullException">Thrown when dbContext is null</exception>
    /// <exception cref="NotSupportedException">Thrown when database provider is not SQL Server</exception>
    public async Task<ViewSyncResult> SyncViewsAsync(
        DbContext dbContext,
        Assembly assembly = null,
        string resourceNamespace = null,
        CancellationToken cancellationToken = default)
    {
        if (dbContext == null)
            throw new ArgumentNullException(nameof(dbContext));

        if (!dbContext.Database.IsSqlServer())
            throw new NotSupportedException("View synchronization is only supported for SQL Server databases.");

        assembly ??= typeof(SyncService).Assembly;

        // Default namespace for embedded resources
        resourceNamespace ??= $"{assembly.GetName().Name}.Views";

        _logger.LogInformation("Starting view synchronization from embedded resources in assembly: {Assembly}, namespace: {Namespace}",
            assembly.GetName().Name, resourceNamespace);

        return await SyncDatabaseObjectsAsync<ViewSyncResult>(
            dbContext, assembly, resourceNamespace, "view", "views",
            GetDatabaseViewsAsync, DropViewAsync,
            @"(?:CREATE|ALTER)\s+VIEW\s+(?:\[?(?<schema>\w+)\]?\.)?\[?(?<name>\w+)\]?",
            cancellationToken);
    }

    /// <summary>
    /// Generic method to synchronize database objects from embedded resources.
    /// </summary>
    private async Task<T> SyncDatabaseObjectsAsync<T>(
        DbContext dbContext,
        Assembly assembly,
        string resourceNamespace,
        string objectTypeName,
        string objectTypePluralName,
        Func<DbContext, CancellationToken, Task<List<string>>> getDatabaseObjects,
        Func<DbContext, string, CancellationToken, Task> dropObject,
        string nameExtractionPattern,
        CancellationToken cancellationToken) where T : DatabaseObjectSyncResult, new()
    {
        var result = new T();

        try
        {
            // Get all embedded SQL resources
            var sqlResources = GetEmbeddedSqlResources(assembly, resourceNamespace);
            var resourceObjects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Extract object names from embedded resources
            foreach (var resourceName in sqlResources)
            {
                var objectName = await ExtractObjectNameFromResourceAsync(assembly, resourceName, nameExtractionPattern);
                if (!string.IsNullOrEmpty(objectName))
                {
                    resourceObjects.Add(objectName);
                }
            }

            // Get existing objects from database
            var existingObjects = await getDatabaseObjects(dbContext, cancellationToken);

            // Drop objects that exist in database but not in embedded resources
            var objectsToDrop = existingObjects.Except(resourceObjects, StringComparer.OrdinalIgnoreCase);
            foreach (var objectName in objectsToDrop)
            {
                await dropObject(dbContext, objectName, cancellationToken);
                result.DroppedCount++;
                _logger.LogInformation("Dropped {ObjectType}: {ObjectName}", objectTypeName, objectName);
            }

            // Create/alter objects from embedded resources
            foreach (var resourceName in sqlResources)
            {
                var objectName = await ExtractObjectNameFromResourceAsync(assembly, resourceName, nameExtractionPattern);
                if (string.IsNullOrEmpty(objectName))
                {
                    _logger.LogWarning("Could not extract {ObjectType} name from resource: {ResourceName}", objectTypeName, resourceName);
                    result.ErrorCount++;
                    continue;
                }

                try
                {
                    await ExecuteSqlResourceAsync(dbContext, assembly, resourceName, cancellationToken);

                    if (existingObjects.Contains(objectName, StringComparer.OrdinalIgnoreCase))
                    {
                        result.AlteredCount++;
                        _logger.LogInformation("Altered {ObjectType}: {ObjectName} from resource: {ResourceName}", objectTypeName, objectName, resourceName);
                    }
                    else
                    {
                        result.CreatedCount++;
                        _logger.LogInformation("Created {ObjectType}: {ObjectName} from resource: {ResourceName}", objectTypeName, objectName, resourceName);
                    }
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    _logger.LogError(ex, "Error executing SQL resource: {ResourceName}", resourceName);
                }
            }

            _logger.LogInformation("{ObjectTypePluralName} synchronization completed. Created: {Created}, Altered: {Altered}, Dropped: {Dropped}, Errors: {Errors}",
                objectTypePluralName.First().ToString().ToUpper() + objectTypePluralName.Substring(1),
                result.CreatedCount, result.AlteredCount, result.DroppedCount, result.ErrorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during {ObjectType} synchronization", objectTypeName);
            throw;
        }

        return result;
    }

    /// <summary>
    /// Gets all embedded SQL resource names from the specified assembly and namespace.
    /// </summary>
    /// <param name="assembly">Assembly to search</param>
    /// <param name="resourceNamespace">Namespace prefix for resources</param>
    /// <returns>List of SQL resource names</returns>
    private static List<string> GetEmbeddedSqlResources(Assembly assembly, string resourceNamespace)
    {
        return assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(resourceNamespace, StringComparison.OrdinalIgnoreCase) &&
                          name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name)
            .ToList();
    }

    /// <summary>
    /// Extracts the stored procedure name from an embedded SQL resource.
    /// </summary>
    /// <param name="assembly">Assembly containing the resource</param>
    /// <param name="resourceName">Name of the embedded resource</param>
    /// <returns>The stored procedure name or null if not found</returns>
    private async Task<string> ExtractProcedureNameFromResourceAsync(Assembly assembly, string resourceName)
    {
        return await ExtractObjectNameFromResourceAsync(assembly, resourceName,
            @"(?:CREATE|ALTER)\s+PROC(?:EDURE)?\s+(?:\[?(?<schema>\w+)\]?\.)?\[?(?<name>\w+)\]?");
    }

    /// <summary>
    /// Extracts the database object name from an embedded SQL resource using the specified pattern.
    /// </summary>
    /// <param name="assembly">Assembly containing the resource</param>
    /// <param name="resourceName">Name of the embedded resource</param>
    /// <param name="nameExtractionPattern">Regex pattern to extract the object name</param>
    /// <returns>The database object name or null if not found</returns>
    private async Task<string> ExtractObjectNameFromResourceAsync(Assembly assembly, string resourceName, string nameExtractionPattern)
    {
        try
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                _logger.LogWarning("Could not find embedded resource: {ResourceName}", resourceName);
                return null;
            }

            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            // Pattern to match CREATE/ALTER statements
            var match = Regex.Match(content, nameExtractionPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            if (match.Success)
            {
                var schema = match.Groups["schema"].Value;
                var name = match.Groups["name"].Value;

                // If no schema specified, assume dbo
                if (string.IsNullOrEmpty(schema))
                    schema = "dbo";

                return $"{schema}.{name}";
            }

            // Fallback: try to extract from resource name (e.g., TestingDemo.Entities.StoredProcedures.dbo.Process_Users.sql)
            var resourceFileName = resourceName.Split('.').TakeLast(3).ToArray(); // Take last 3 parts: [dbo, Process_Users, sql]
            if (resourceFileName.Length >= 3 && resourceFileName[2] == "sql")
            {
                return $"{resourceFileName[0]}.{resourceFileName[1]}";
            }

            // Final fallback: assume dbo schema
            var lastTwoParts = resourceName.Split('.').TakeLast(2).ToArray();
            if (lastTwoParts.Length >= 2 && lastTwoParts[1] == "sql")
            {
                return $"dbo.{lastTwoParts[0]}";
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting object name from resource: {ResourceName}", resourceName);
            return null;
        }
    }

    /// <summary>
    /// Gets all user-defined stored procedures from the database.
    /// </summary>
    /// <param name="dbContext">The database context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of stored procedure names in schema.name format</returns>
    private async Task<List<string>> GetDatabaseStoredProceduresAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT SCHEMA_NAME(schema_id) + '.' + name as ObjectName
            FROM sys.procedures
            WHERE type = 'P' AND is_ms_shipped = 0
            ORDER BY SCHEMA_NAME(schema_id), name";

        return await ExecuteObjectQueryAsync(dbContext, sql, cancellationToken);
    }

    /// <summary>
    /// Gets all user-defined functions from the database.
    /// </summary>
    /// <param name="dbContext">The database context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of function names in schema.name format</returns>
    private async Task<List<string>> GetDatabaseFunctionsAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT SCHEMA_NAME(schema_id) + '.' + name as ObjectName
            FROM sys.objects
            WHERE type IN ('FN', 'IF', 'TF') AND is_ms_shipped = 0
            ORDER BY SCHEMA_NAME(schema_id), name";

        return await ExecuteObjectQueryAsync(dbContext, sql, cancellationToken);
    }

    /// <summary>
    /// Gets all user-defined views from the database.
    /// </summary>
    /// <param name="dbContext">The database context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of view names in schema.name format</returns>
    private async Task<List<string>> GetDatabaseViewsAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT SCHEMA_NAME(schema_id) + '.' + name as ObjectName
            FROM sys.views
            WHERE is_ms_shipped = 0
            ORDER BY SCHEMA_NAME(schema_id), name";

        return await ExecuteObjectQueryAsync(dbContext, sql, cancellationToken);
    }

    /// <summary>
    /// Executes a query to get database objects.
    /// </summary>
    /// <param name="dbContext">The database context</param>
    /// <param name="sql">SQL query to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of object names</returns>
    private async Task<List<string>> ExecuteObjectQueryAsync(DbContext dbContext, string sql, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        var wasOpen = connection.State == ConnectionState.Open;

        try
        {
            if (!wasOpen)
                await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = sql;

            var objects = new List<string>();
            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                objects.Add(reader.GetString(0));
            }

            return objects;
        }
        finally
        {
            if (!wasOpen && connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }
    }

    /// <summary>
    /// Drops a stored procedure from the database.
    /// </summary>
    /// <param name="dbContext">The database context</param>
    /// <param name="procedureName">Name of the procedure to drop</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task DropStoredProcedureAsync(DbContext dbContext, string procedureName, CancellationToken cancellationToken)
    {
        var sql = $"DROP PROCEDURE IF EXISTS [{procedureName.Replace(".", "].[")}]";
        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    /// <summary>
    /// Drops a function from the database.
    /// </summary>
    /// <param name="dbContext">The database context</param>
    /// <param name="functionName">Name of the function to drop</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task DropFunctionAsync(DbContext dbContext, string functionName, CancellationToken cancellationToken)
    {
        var sql = $"DROP FUNCTION IF EXISTS [{functionName.Replace(".", "].[")}]";
        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    /// <summary>
    /// Drops a view from the database.
    /// </summary>
    /// <param name="dbContext">The database context</param>
    /// <param name="viewName">Name of the view to drop</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task DropViewAsync(DbContext dbContext, string viewName, CancellationToken cancellationToken)
    {
        var sql = $"DROP VIEW IF EXISTS [{viewName.Replace(".", "].[")}]";
        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    /// <summary>
    /// Executes a SQL embedded resource against the database.
    /// </summary>
    /// <param name="dbContext">The database context</param>
    /// <param name="assembly">Assembly containing the resource</param>
    /// <param name="resourceName">Name of the embedded resource</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task ExecuteSqlResourceAsync(DbContext dbContext, Assembly assembly, string resourceName, CancellationToken cancellationToken)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");
        }

        using var reader = new StreamReader(stream);
        var sql = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(sql))
        {
            _logger.LogWarning("SQL resource is empty: {ResourceName}", resourceName);
            return;
        }

        // Split by GO statements and execute each batch
        var batches = SplitSqlBatches(sql);

        foreach (var batch in batches)
        {
            if (!string.IsNullOrWhiteSpace(batch))
            {
                await dbContext.Database.ExecuteSqlRawAsync(batch, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Splits SQL content by GO statements to handle multiple batches.
    /// </summary>
    /// <param name="sql">SQL content</param>
    /// <returns>Array of SQL batches</returns>
    private static string[] SplitSqlBatches(string sql)
    {
        return Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                   .Where(batch => !string.IsNullOrWhiteSpace(batch))
                   .ToArray();
    }
}
