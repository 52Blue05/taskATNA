using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations;

public class RolePositionFeatureMenuConfiguration : IEntityTypeConfiguration<RolePositionFeatureMenu>
{
	public void Configure(EntityTypeBuilder<RolePositionFeatureMenu> builder)
	{
		builder.ToTable("RolePositionFeatureMenus");
		builder.HasOne(t => t.RolePosition).WithMany(t => t.RolePositionFeatureMenus).HasForeignKey(t => t.RolePositionId);
		builder.HasOne(t => t.FeatureMenu).WithMany(t => t.RolePositionFeatureMenus).HasForeignKey(t => t.FeatureMenuId);
	}
}
