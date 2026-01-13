using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
    public class OpportunityHistoryConfiguration : IEntityTypeConfiguration<OpportunityHistory>
    {
        public void Configure(EntityTypeBuilder<OpportunityHistory> builder)
        {
            builder.ToTable("OpportunityHistories");
            builder.HasOne(t => t.Opportunity).WithMany(t => t.OpportunityHistories).HasForeignKey(t => t.OpportunityId);
            builder.HasOne(t => t.ApplicationUser).WithMany(t => t.OpportunityHistories).HasForeignKey(t => t.ApplicationUserId);
        }
    }
}
