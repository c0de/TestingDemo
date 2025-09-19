// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestingDemo.Entities.Models;

namespace TestingDemo.Entities.Configurations;

/// <summary>
/// Configuration for the <see cref="User"/> entity.
/// </summary>
internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Add configuration
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);
        builder.Property(x => x.Role)
            .IsRequired()
            .HasMaxLength(20);
        builder.HasMany(x => x.Dashboards)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add indexes for performance
        //builder.HasIndex(x => x.Email).IsUnique().HasDatabaseName("IX_Users_Email");
        //builder.HasIndex(x => x.Role).HasDatabaseName("IX_Users_Role");
        //builder.HasIndex(x => x.DeletedAt).HasDatabaseName("IX_Users_DeletedAt");
    }
}
