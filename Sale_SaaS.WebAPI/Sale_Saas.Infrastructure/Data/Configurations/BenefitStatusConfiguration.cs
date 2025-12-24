using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Sale_Saas.Infrastructure.Data.Configurations
{
    public class BenefitStatusConfiguration : IEntityTypeConfiguration<BenefitStatus>
    {
        public void Configure(EntityTypeBuilder<BenefitStatus> builder)
        {
            builder.ToTable("BenefitStatuses");
        }
    }
}
