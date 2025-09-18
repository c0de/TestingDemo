// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestingDemo.Entities.Models;

namespace TestingDemo.Entities.Configurations;

/// <summary>
/// Configuration for the <see cref="ActiveUsersView"/> entity.
/// </summary>
public class ActiveUsersViewConfiguration : IEntityTypeConfiguration<ActiveUsersView>
{
    public void Configure(EntityTypeBuilder<ActiveUsersView> builder)
    {
        builder.ToView("ActiveUsers", "dbo")
            .HasKey(x => new { x.Id });
    }
}
