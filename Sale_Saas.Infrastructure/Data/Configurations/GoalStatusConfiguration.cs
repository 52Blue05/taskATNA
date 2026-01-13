using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
    public class GoalStatusConfiguration : IEntityTypeConfiguration<GoalStatus>
    {
        public void Configure(EntityTypeBuilder<GoalStatus> builder)
        {
            builder.ToTable("GoalStatuses");
        }
    }
}
