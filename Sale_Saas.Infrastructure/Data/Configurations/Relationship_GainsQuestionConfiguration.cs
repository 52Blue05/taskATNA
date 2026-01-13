using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
    public class Relationship_GainsQuestionConfiguration : IEntityTypeConfiguration<Relationship_GainsQuestion>
    {
        public void Configure(EntityTypeBuilder<Relationship_GainsQuestion> builder)
        {
            builder.ToTable("Relationship_GainsQuestions");
            builder.HasOne(t => t.Relationship).WithMany(t => t.Relationship_GainsQuestion).HasForeignKey(t => t.RelationshipId);
            builder.HasOne(t => t.GainsQuestion).WithMany(t => t.Relationship_GainsQuestion).HasForeignKey(t => t.GainsQuestionId);
        }
    }
}
