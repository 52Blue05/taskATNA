using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations;

public class CriteriaConfiguration : IEntityTypeConfiguration<Criteria>
{
    public void Configure(EntityTypeBuilder<Criteria> builder)
    {
        builder.ToTable("Criterias");
    }
}
