using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
    public class OppotunityConfiguration : IEntityTypeConfiguration<Opportunity>
    {
        public void Configure(EntityTypeBuilder<Opportunity> builder)
        {
            builder.ToTable("Opportunities");
            builder.HasOne(t => t.ApplicationUser).WithMany(t => t.Opportunities).HasForeignKey(t => t.ApplicationUserId);
            builder.HasOne(t => t.OpportunityStatus).WithMany(t => t.Opportunities).HasForeignKey(t => t.OpportunityStatusId);
        }
    }
}
