// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using TestingDemo.Entities;
using TestingDemo.Entities.Models;

namespace TestingDemo.Tests;

/// <summary>
/// Model for testing an api endpoint.
/// </summary>
public class TestingInstance
{
    /// <summary>
    /// Injected <see cref="IDemoDbContext"/>.
    /// </summary>
    public required IDemoDbContext Repository { get; set; }
    /// <summary>
    /// Http Client to test.
    /// </summary>
    public required HttpClient Api { get; set; }
    /// <summary>
    /// Collection of Injected Services.
    /// </summary>
    public required IServiceProvider Services { get; set; }
    /// <summary>
    /// Authenticated User.
    /// </summary>
    public User? User { get; set; }
}
