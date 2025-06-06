﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestingDemo.Entities.Models;

namespace TestingDemo.Entities.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
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
    }
}
