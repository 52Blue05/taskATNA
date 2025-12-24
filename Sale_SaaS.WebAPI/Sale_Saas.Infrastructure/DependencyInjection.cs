using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Sale_Saas.Application.Common.Interfaces;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services.TenantService;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Infrastructure.Authentication;
using Sale_Saas.Infrastructure.Extensions;
using Sale_Saas.Infrastructure.Services;
using Sale_Saas.Infrastructure.Services.ApplicationService;
using Sale_Saas.Infrastructure.Services.Identity;
using Sale_Saas.Infrastructure.Services.TenantService;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Sale_SaasDbConnection");
            var strConnectCore = configuration.GetConnectionString("Sale_SaasDbConnectionCore");

            Guard.Against.Null(connectionString, message: "Connection string 'Sale_SaasDbConnection' not found.");

            services.AddScoped<ICurrentTenantService, CurrentTenantService>();
            services.AddDbContext<TenantDbContext>(options => options.UseNpgsql(connectionString));
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(strConnectCore));
            services.AddAndMigrateTenantDatabases(configuration);
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
            services.AddTransient<ITenantService, TenantService>();
            services.AddTransient<ITenantApplicationService, TenantApplicationService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IModuleTenantService, ModuleTenantService>();
            services.AddTransient<ITenantDbContextInitialiser, TenantDbContextInitialiser>();

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            services.AddTransient<IApplicationDbContextInitialiser, ApplicationDbContextInitialiser>();
            services.AddScoped<PermisionTypeConstant>();
            services.AddTransient<IUserAdminService, UserAdminService>();

            // Truy cập IdentityOptions
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 3;
                options.Password.RequiredUniqueChars = 1;
            });

            var endpoint = "10.0.0.16:9000";
            var accessKey = "eEvBj2Hb2V39NwW1EbC6";
            var secretKey = "eNc3ZCgt2hCUwu5xw7ebrLs80lzSycSeEvIOvLJa";
            var secure = false;

            // ✅ Giữ nguyên logic cũ
            services.AddMinio(configureClient => configureClient
                        .WithEndpoint(endpoint)
                        .WithCredentials(accessKey, secretKey)
                        .WithSSL(secure)
                        .Build());

            // ✅ BỔ SUNG: map IMinioClient để FileStorageService inject được
            services.AddSingleton<IMinioClient>(sp =>
            {
                // dùng cùng config để tránh lệch cấu hình
                return new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(accessKey, secretKey)
                    .WithSSL(secure)
                    .Build();
            });

            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
            services.AddSingleton(TimeProvider.System);
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<IEventLogService, EventLogService>();
            services.AddTransient<IApplicationRoleDetailService, ApplicationRoleDetailService>();
            services.AddTransient<IApplicationRoleService, ApplicationRoleService>();
            services.AddTransient<IApplicationUserService, ApplicationUserService>();
            services.AddTransient<IFeaturePermissionService, FeaturePermissionServices>();
            services.AddTransient<IFileStorageService, FileStorageService>();
            services.AddTransient<IInternalService, InternalService>();
            services.AddTransient<ISendEmailNotificationService, SendEmailNotificationService>();
            services.AddTransient<ILoggerService, LoggerService>();
            services.AddTransient<ITrackingLogService, TrackingLogService>();
            services.AddTransient<IGroupTenantService, GroupTenantService>();
            services.AddTransient<IPlanService, PlanServiceService>();
            services.AddTransient<IOtpSendService, OtpSendService>();
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
