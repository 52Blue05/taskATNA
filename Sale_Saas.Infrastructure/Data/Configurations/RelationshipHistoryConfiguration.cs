using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
     public class RelationshipHistoryConfiguration : IEntityTypeConfiguration<RelationshipHistory>
     {
          public void Configure(EntityTypeBuilder<RelationshipHistory> builder)
          {
               builder.ToTable("RelationshipHistories");
               builder.HasOne(x => x.Relationship).WithMany(x => x.RelationshipHistories).HasForeignKey(x => x.RelationshipId);
               builder.HasOne(x => x.PreviousLevel).WithMany(x => x.RelationshipHistoriesPrevious).HasForeignKey(x => x.PreviousLevelId);
               builder.HasOne(x => x.UpdatedLevel).WithMany(x => x.RelationshipHistoriesUpdated).HasForeignKey(x => x.UpdatedLevelId);
          }
     }
}
