using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sale_Saas.Domain.Entities.Tenant;

namespace Sale_Saas.Infrastructure.Extensions
{
    public static class MultipleDatabaseExtensions
    {
        public static IServiceCollection AddAndMigrateTenantDatabases(this IServiceCollection services, IConfiguration configuration)
        {

            // Tenant Db Context (reference context) - get a list of tenants
            using (var scopeTenant = services.BuildServiceProvider().CreateScope())
            {
                TenantDbContext tenantDbContext = scopeTenant.ServiceProvider.GetRequiredService<TenantDbContext>();

                if (tenantDbContext.Database.GetPendingMigrations().Any())
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Applying BaseDb Migrations.");
                    Console.ResetColor();
                    try
                    {
                        tenantDbContext.Database.Migrate(); // apply migrations on baseDbContext
                    }
                    catch { }
                }
                List<Tenant> tenantsInDb = new List<Tenant>();
                try
                {
                    tenantsInDb = tenantDbContext.Tenants.Where(s => s.DeleteFlag != true).ToList();
                }
                catch (Exception ex) { }
                if (tenantsInDb.Count <= 0)
                {
                    return services;
                }
                string defaultConnectionString = configuration.GetConnectionString("Sale_SaasDbConnectionCore") ?? ""; // read default connection string from appsettings.json

                foreach (Tenant tenant in tenantsInDb) // loop through all tenants, apply migrations on applicationDbContext
                {
                    if (!string.IsNullOrEmpty(tenant.Id) && !string.IsNullOrEmpty(tenant.ConnectionString))
                    {
                        string connectionString = tenant.ConnectionString;

                        using (var scopeApplication = services.BuildServiceProvider().CreateScope())
                        {
                            ApplicationDbContext dbContext = scopeApplication.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            dbContext.Database.SetConnectionString(connectionString);
                            if (dbContext.Database.GetPendingMigrations().Any())
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine($"Applying Migrations for '{tenant.Id}' tenant.");
                                Console.ResetColor();
                                try
                                {
                                    dbContext.Database.Migrate();

                                    //var initialiser = scopeApplication.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
                                    //Task.Run(async () => { await initialiser.SeedAsync(); });
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);

                                }
                            }
                            dbContext.Dispose();
                            scopeApplication.Dispose();
                        }

                    }
                }
                scopeTenant.Dispose();
                return services;
            }

        }

    }
}
