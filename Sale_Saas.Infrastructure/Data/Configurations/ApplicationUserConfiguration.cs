using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("ApplicationUsers");

        //builder.HasOne(t => t.GioiTinh).WithMany(t => t.ApplicationUsers).HasForeignKey(t => t.GioiTinhId);
        builder.HasOne(t => t.ApplicationUserStatus).WithMany(t => t.ApplicationUsers).HasForeignKey(t => t.ApplicationUserStatusId);
		//builder.HasOne(t => t.DanToc).WithMany(t => t.ApplicationUsers).HasForeignKey(t => t.DanTocId);
		//builder.HasOne(t => t.PhongBan).WithMany(t => t.ApplicationUsers).HasForeignKey(t => t.PhongBanId);
		//builder.HasOne(t => t.NganHang).WithMany(t => t.ApplicationUsers).HasForeignKey(t => t.NganHangId);
	}
}
