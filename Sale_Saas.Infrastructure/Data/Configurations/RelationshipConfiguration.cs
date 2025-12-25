using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
    public class RelationshipConfiguration : IEntityTypeConfiguration<Relationship>
    {
        public void Configure(EntityTypeBuilder<Relationship> builder)
        {
            builder.ToTable("Relationships");
            builder.HasOne(t => t.CurrentRelationship).WithMany(t => t.CurrentRelationships).HasForeignKey(t => t.CurrentRelationshipId);
            builder.HasOne(t => t.TargetRelationship).WithMany(t => t.TargetRelationships).HasForeignKey(t => t.TargetRelationshipId);
            builder.HasOne(t => t.RelationshipStatus).WithMany(t => t.Relationships).HasForeignKey(t => t.RelationshipStatusId);
            builder.HasOne(t => t.Gains).WithOne(t => t.Relationship).HasForeignKey<Relationship>(t => t.GainsId);
            builder.HasOne(t => t.ApplicationUser).WithMany(t => t.Relationships).HasForeignKey(t => t.ApplicationUserId);
            //builder.HasOne(t => t.DanToc).WithMany(t => t.ApplicationUsers).HasForeignKey(t => t.DanTocId);
            builder.HasOne(t => t.YearToDate).WithMany(t => t.YearToDateRelationships).HasForeignKey(t => t.YearToDateId);
        }
    }
}
