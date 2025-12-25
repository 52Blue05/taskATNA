using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
     public class GainsFamilyConfiguration : IEntityTypeConfiguration<GainsFamily>
     {
          public void Configure(EntityTypeBuilder<GainsFamily> builder)
          {
               builder.ToTable("GainsFamily");

               builder.HasOne(x => x.Gains).WithMany(x => x.GainsFamilies).HasForeignKey(x => x.GainsId);
          }
     }
}
