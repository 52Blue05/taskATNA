using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
    public class RelationshipLevelConfiguration : IEntityTypeConfiguration<RelationshipLevel>
    {
        public void Configure(EntityTypeBuilder<RelationshipLevel> builder)
        {
            builder.ToTable("RelationshipLevels");

            builder.HasMany(x => x.YearToDateRelationships).WithOne(x => x.YearToDate).HasForeignKey(x => x.YearToDateId);
        }
    }
}
