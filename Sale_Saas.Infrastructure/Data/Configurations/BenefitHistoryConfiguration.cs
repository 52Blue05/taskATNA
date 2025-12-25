using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
    public class BenefitHistoryConfiguration : IEntityTypeConfiguration<BenefitHistory>
    {
        public void Configure(EntityTypeBuilder<BenefitHistory> builder)
        {
            builder.ToTable("BenefitHistories");
            builder.HasOne(t => t.Benefit).WithMany(t => t.BenefitHistories).HasForeignKey(t => t.BenefitId);
			builder.HasOne(t => t.PreviousStatus).WithMany(t => t.BenefitHistoriesPrevious).HasForeignKey(t => t.PreviousStatusId);
			builder.HasOne(t => t.UpdatedStatus).WithMany(t => t.BenefitHistoriesUpdated).HasForeignKey(t => t.UpdatedStatusId);
		}
    }
}
