using Sale_Saas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sale_Saas.Infrastructure.Data.Configurations;

public class ApplicationRoleDetailConfiguration : IEntityTypeConfiguration<ApplicationRoleDetail>
{
    public void Configure(EntityTypeBuilder<ApplicationRoleDetail> builder)
    {
        builder.ToTable("ApplicationRoleDetails");

        //builder.HasOne(t => t.Menu).WithMany(pc => pc.RoleDetails).HasForeignKey(pc => pc.MenuId);
        builder.HasOne(t => t.ApplicationRole).WithMany(pc => pc.RoleDetails).HasForeignKey(pc => pc.ApplicationRoleId);
		builder.HasOne(t => t.Feature).WithMany(pc => pc.RoleDetails).HasForeignKey(pc => pc.FeatureId);
	}
}
