// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace TestingDemo.Api.Users.Queries;

public class UserResponse
{
    /// <summary>
    /// Id of the user.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// First name of the user.
    /// </summary>
    public string FirstName { get; set; }
    /// <summary>
    /// Last name of the user.
    /// </summary>
    public string LastName { get; set; }
    /// <summary>
    /// Email address of the user.
    /// </summary>
    public string Email { get; set; }
    /// <summary>
    /// DateTime the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Indicates if the user is active (not deleted).
    /// </summary>
    public bool IsActive { get; set; }
}
