using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
     public class RelationshipCustomerConfiguration : IEntityTypeConfiguration<RelationshipCustomer>
     {
          public void Configure(EntityTypeBuilder<RelationshipCustomer> builder)
          {
               builder.ToTable("RelationshipCustomers");

               builder.HasMany(x => x.Relationships).WithOne(x => x.RelationshipCustomer).HasForeignKey(x => x.RelationshipCustomerId);
          }
     }
}
