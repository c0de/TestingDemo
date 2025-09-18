// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestingDemo.Entities.Models;

namespace TestingDemo.Entities.Configurations;

/// <summary>
/// Configuration for the <see cref="UserDashboard"/> entity.
/// </summary>
public class UserDashboardConfiguration : IEntityTypeConfiguration<UserDashboard>
{
    public void Configure(EntityTypeBuilder<UserDashboard> builder)
    {
        builder.HasKey(x => new { x.UserId, x.DashboardId });
    }
}
