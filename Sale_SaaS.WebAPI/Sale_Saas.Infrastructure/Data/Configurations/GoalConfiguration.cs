using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
    public class GoalConfiguration : IEntityTypeConfiguration<Goal>
    {
        public void Configure(EntityTypeBuilder<Goal> builder)
        {
            builder.ToTable("Goals");
            builder.HasOne(t => t.UserSuggest).WithMany(t => t.Goals).HasForeignKey(t => t.UserSuggestId);
            builder.HasOne(t => t.GoalStatus).WithMany(t => t.Goals).HasForeignKey(t => t.GoalStatusId);
            builder.HasMany(x => x.TargetFluctuations).WithOne(x => x.Goal).HasForeignKey(x => x.GoalId);
            builder.HasOne(t => t.Criteria).WithMany(t => t.Goals).HasForeignKey(t => t.CriteriaId);
        }
    }
}
