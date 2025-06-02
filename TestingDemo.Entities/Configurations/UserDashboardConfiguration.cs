using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestingDemo.Entities.Models;

namespace TestingDemo.Entities.Configurations;

public class UserDashboardConfiguration : IEntityTypeConfiguration<UserDashboard>
{
    public void Configure(EntityTypeBuilder<UserDashboard> builder)
    {
        builder.HasKey(x => new { x.UserId, x.DashboardId });
    }
}
