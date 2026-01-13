using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations;

public class GainsQuestionConfiguration : IEntityTypeConfiguration<GainsQuestion>
{
    public void Configure(EntityTypeBuilder<GainsQuestion> builder)
    {
        builder.ToTable("GainsQuestions");

    }
}
