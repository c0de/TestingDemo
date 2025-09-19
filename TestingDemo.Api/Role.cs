// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace TestingDemo.Api;

/// <summary>
/// Roles for the application.
/// </summary>
public enum Role
{
    /// <summary>
    /// Administrator.
    /// </summary>
    Admin = 1,
    /// <summary>
    /// User
    /// </summary>
    User = 2
}

/// <summary>
/// Extension methods for Role enum operations.
/// </summary>
public static class RoleExtensions
{
    /// <summary>
    /// Converts Role enum values to their string representations.
    /// </summary>
    /// <param name="roles">The roles to convert.</param>
    /// <returns>Array of role names as strings.</returns>
    public static string[] ToStringArray(params Role[] roles)
    {
        return roles.Select(r => r.ToString()).ToArray();
    }

    /// <summary>
    /// Gets all Role enum values as string array.
    /// </summary>
    /// <returns>Array of all role names as strings.</returns>
    public static string[] GetAllRoleNames()
    {
        return Enum.GetNames<Role>();
    }
}
