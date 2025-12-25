using Sale_Saas.Domain.Entities.Tenant;
using System.Reflection;

namespace Sale_Saas.Infrastructure.Data
{
    public class TenantDbContext : DbContext
    {
        public TenantDbContext(DbContextOptions<TenantDbContext> option) : base(option)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
		public DbSet<UserStatus> UserStatus { get; set; }
		public DbSet<Tenant> Tenants { get; set; }
        public DbSet<UserTenant> UserTenants { get; set; }
		public DbSet<TrackingLog> TrackingLogs { get; set; }
		public DbSet<ModuleTenant> ModuleTenants { get; set; }
        public DbSet<UserAdmin> UserAdmins { get; set; }
        public DbSet<User> Users { get; set; }
		public DbSet<Notification> Notifications { get; set; }
		public DbSet<PlanService> PlanServices { get; set; }
		public DbSet<PlanServiceModuleTenant> PlanServiceModuleTenants { get; set; }
		public DbSet<GroupTenant> GroupTenants { get; set; }
		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OtpSend> OtpSends { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
        // On Save Changes - write tenant Id to table
        public override int SaveChanges()
        {         
            var result = base.SaveChanges();
            return result;
        }
    }
}
