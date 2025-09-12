using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestingDemo.Entities.Models;

namespace TestingDemo.Entities.Configurations;

/// <summary>
/// Configuration for the <see cref="Dashboard"/> entity.
/// </summary>
internal class DashboardConfiguration : IEntityTypeConfiguration<Dashboard>
{
    public void Configure(EntityTypeBuilder<Dashboard> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.Description)
            .HasMaxLength(500);
        builder.HasMany(x => x.Users)
            .WithOne(x => x.Dashboard)
            .HasForeignKey(x => x.DashboardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
