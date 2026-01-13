using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
     public class BenefitConfiguration : IEntityTypeConfiguration<Benefit>
     {
          public void Configure(EntityTypeBuilder<Benefit> builder)
          {
               builder.ToTable("Benefits");
               builder.HasOne(t => t.BenefitStatus).WithMany(t => t.Benefits).HasForeignKey(t => t.BenefitStatusId);
               builder.HasOne(t => t.ApplicationUser).WithMany(t => t.Benefits).HasForeignKey(t => t.ApplicationUserId);

               builder.HasMany(x => x.TargetFluctuations).WithOne(x => x.Benefit).HasForeignKey(x => x.BenefitId);
          }
     }
}
