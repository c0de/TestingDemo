// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace TestingDemo.Entities.Models;

public class UserDashboard
{
    public int UserId { get; set; }
    public int DashboardId { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; }
    public virtual Dashboard Dashboard { get; set; }
}
