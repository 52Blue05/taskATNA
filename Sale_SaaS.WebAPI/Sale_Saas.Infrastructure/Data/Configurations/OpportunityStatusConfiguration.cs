using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
    public class OpportunityStatusConfiguration : IEntityTypeConfiguration<OpportunityStatus>
    {
        public void Configure(EntityTypeBuilder<OpportunityStatus> builder)
        {
            builder.ToTable("OpportunityStatuses");
        }
    }
}
