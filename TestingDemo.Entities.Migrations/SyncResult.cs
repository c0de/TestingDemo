// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace TestingDemo.Entities.Migrations;

/// <summary>
/// Base class for database object synchronization results.
/// </summary>
public class SyncResult
{
    /// <summary>
    /// Number of objects created.
    /// </summary>
    public int CreatedCount { get; set; }

    /// <summary>
    /// Number of objects altered.
    /// </summary>
    public int AlteredCount { get; set; }

    /// <summary>
    /// Number of objects dropped.
    /// </summary>
    public int DroppedCount { get; set; }

    /// <summary>
    /// Number of errors encountered.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Adds the counts from another sync result to this one.
    /// </summary>
    /// <param name="other">The other sync result to add</param>
    public void Add(SyncResult other)
    {
        if (other != null)
        {
            CreatedCount += other.CreatedCount;
            AlteredCount += other.AlteredCount;
            DroppedCount += other.DroppedCount;
            ErrorCount += other.ErrorCount;
        }
    }
}


/// <summary>
/// Result of stored procedure synchronization operation.
/// </summary>
public class StoredProcedureSyncResult : SyncResult
{
}

/// <summary>
/// Result of function synchronization operation.
/// </summary>
public class FunctionSyncResult : SyncResult
{
}

/// <summary>
/// Result of view synchronization operation.
/// </summary>
public class ViewSyncResult : SyncResult
{
}
