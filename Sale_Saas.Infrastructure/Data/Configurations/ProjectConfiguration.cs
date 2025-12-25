using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sale_Saas.Infrastructure.Data.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("Projects");
            builder.HasOne(t => t.ProjectStatus).WithMany(t => t.Projects).HasForeignKey(t => t.ProjectStatusId);
            builder.HasOne(t => t.ApplicationUser).WithMany(t => t.Projects).HasForeignKey(t => t.ApplicationUserId);
            builder.HasOne(t => t.Contract).WithMany(t => t.Projects).HasForeignKey(t => t.ContractId);
        }
    }
}
