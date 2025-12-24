using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations;

public class MailServerInfoConfiguration : IEntityTypeConfiguration<MailServerInfo>
{
    public void Configure(EntityTypeBuilder<MailServerInfo> builder)
    {
        builder.ToTable("MailServerInfos");

    }
}
