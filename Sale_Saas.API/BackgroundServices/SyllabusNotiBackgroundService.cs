
using Sale_Saas.Application.Common.Interfaces;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Infrastructure.Data;
using System.Data.Entity;
using Sale_Saas.Application.Models.Notification;
using Sale_Saas.Infrastructure.Services.Identity;
using System.Reflection;
using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Domain.Entities.Tenant;

namespace Sale_Saas.API.BackgroundServices
{
    public class SyllabusNotiBackgroundService : BackgroundService
    {
        private readonly TimeSpan _timeSpan = TimeSpan.FromHours(24);
        private readonly IServiceProvider _serviceProvider;

        public SyllabusNotiBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var now = DateTime.Now;
            var nextMidnight = now.Date.AddDays(1);
            var initialDelay = nextMidnight - now;

            await Task.Delay(initialDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;
                    var context = serviceProvider.GetRequiredService<TenantDbContext>();
                    var apcontext = serviceProvider.GetRequiredService<IApplicationDbContext>();
                    var userService = serviceProvider.GetRequiredService<IApplicationUserService>();
                    var notificationService = serviceProvider.GetRequiredService<INotificationService>();
                    var loggerService = serviceProvider.GetRequiredService<ILoggerService>();

                    await SendNotification(context, apcontext, loggerService, userService, notificationService);

                    await context.DisposeAsync();
                    await apcontext.DisposeAsync();
                    scope.Dispose();
                }

                await Task.Delay(_timeSpan, stoppingToken);
            }
        }

        private async Task SendNotification(TenantDbContext _context, IApplicationDbContext _apContext, 
                                            ILoggerService loggerService, IApplicationUserService userService,
                                            INotificationService notificationService)
        {
            try
            {
                var listTenant = await _context.GroupTenants
                                               .Where(g => g.DeleteFlag != true)
                                               .SelectMany(g => g.Tenants != null
                                                                ? g.Tenants.Where(t => t.DeleteFlag != true)
                                                                : Enumerable.Empty<Tenant>())
                                               .Distinct()
                                               .ToListAsync();


                if (listTenant.Any())
                {
                    List<PushNotificationRequest> notifications = new List<PushNotificationRequest>();

                    foreach (var tenant in listTenant)
                    {
                        userService.SetConnectDB(tenant.ConnectionString ?? "");

                        var listUserSyllabus = (from user in _apContext.ApplicationUsers
                                                      join userSy in _apContext.ApplicationUserSyllabus on user.Id equals userSy.ApplicationUserId
                                                      join sy in _apContext.Syllabus on userSy.SyllabusId equals sy.Id
                                                      where user.DeleteFlag != true && sy.DeleteFlag != true && sy.EndTime == DateTime.Now.AddDays(1).Date
                                                      select new
                                                      {
                                                          UserId = user.Id,
                                                          SyllabusName = sy.Name,
                                                          SyllabusId = sy.Id,
                                                      }).ToList();

                        if (listUserSyllabus.Any())
                        {
                            foreach (var item in listUserSyllabus)
                            {
                                var noti = new PushNotificationRequest
                                (
                                    NotificationAction.SyllabusMsg.Title,
                                    NotificationAction.SyllabusMsg.Expired(item.SyllabusName),
                                    NotificationAction.SyllabusMsg.Type,
                                    item.SyllabusId.ToString(),
                                    item.UserId,
                                    tenant.Id
                                );
                                notifications.Add(noti);
                            }
                        }

                        userService.ClearConnectDB();
                    }

                    if (notifications.Any())
                    {
                        await notificationService.PushAsync(notifications);
                    }
                }
            }
            catch (Exception ex)
            {
                loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
            }
        }
    }
}
