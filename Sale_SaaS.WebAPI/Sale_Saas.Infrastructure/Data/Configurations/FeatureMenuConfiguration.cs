using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
	public class FeatureMenuConfiguration : IEntityTypeConfiguration<FeatureMenu>
	{
		public void Configure(EntityTypeBuilder<FeatureMenu> builder)
		{
			builder.ToTable("FeatureMenus");
			builder.HasOne(t => t.Feature).WithMany(t => t.FeatureMenus).HasForeignKey(t => t.FeatureId);
			builder.HasOne(t => t.Menu).WithMany(t => t.FeatureMenus).HasForeignKey(t => t.MenuId);
		}
	}
}
