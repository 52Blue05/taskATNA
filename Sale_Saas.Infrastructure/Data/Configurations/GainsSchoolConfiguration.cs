using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
     public class GainsSchoolConfiguration : IEntityTypeConfiguration<GainsSchool>
     {
          public void Configure(EntityTypeBuilder<GainsSchool> builder)
          {
               builder.ToTable("GainsSchools");

               builder.HasOne(x => x.Gains).WithMany(x => x.GainsSchools).HasForeignKey(x => x.GainsId);
          }
     }
}
