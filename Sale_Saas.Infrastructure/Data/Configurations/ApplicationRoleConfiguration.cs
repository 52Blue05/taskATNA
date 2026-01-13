using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations;

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("ApplicationRoles");
        builder.HasOne(t => t.RolePosition).WithMany(t => t.ApplicationRoles).HasForeignKey(t => t.RolePositionId);
    }
}
