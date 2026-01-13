using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Sale_Saas.Infrastructure.Data.Configurations;
public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

    }
}