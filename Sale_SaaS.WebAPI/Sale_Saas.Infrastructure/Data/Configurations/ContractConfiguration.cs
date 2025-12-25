using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
    public class ContractConfiguration : IEntityTypeConfiguration<Contract>
    {
        public void Configure(EntityTypeBuilder<Contract> builder)
        {
            builder.ToTable("Contracts");
            builder.HasOne(t => t.Customer).WithMany(t => t.Contracts).HasForeignKey(t => t.CustomerId);
            builder.HasOne(t => t.ContractStatus).WithMany(t => t.Contracts).HasForeignKey(t => t.ContractStatusId);
        }
    }
}
