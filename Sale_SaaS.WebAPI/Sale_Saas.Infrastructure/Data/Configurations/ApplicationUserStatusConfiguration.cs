using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations;

public class ApplicationUserStatusConfiguration : IEntityTypeConfiguration<ApplicationUserStatus>
{
    public void Configure(EntityTypeBuilder<ApplicationUserStatus> builder)
    {
        builder.ToTable("ApplicationUserStatuses");
    }
}
