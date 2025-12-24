using Microsoft.EntityFrameworkCore;
using Sale_Saas.Application.Common.Interfaces;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Notification;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Data;
using Sale_Saas.Infrastructure.Services.Identity;
using System.Reflection;

namespace Sale_Saas.API.BackgroundServices;

public class DataExpiredBackgroundService : BackgroundService
{
	private readonly TimeSpan _delay = TimeSpan.FromHours(2);
	private readonly IServiceScopeFactory _serviceProvider;

	public DataExpiredBackgroundService(IServiceScopeFactory serviceProvider)
	{
		_serviceProvider = serviceProvider;
		//_apcontext = apcontext;
		//_userService = userService;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var serviceProvider = scope.ServiceProvider;
				var context = serviceProvider.GetRequiredService<TenantDbContext>();
				var apcontext = serviceProvider.GetRequiredService<IApplicationDbContext>();
				var notificationService = serviceProvider.GetRequiredService<INotificationService>();
				var loggerService = serviceProvider.GetRequiredService<ILoggerService>();

				await SendGoalExpireNoti(apcontext, notificationService, context, loggerService);

				await context.DisposeAsync();
				await apcontext.DisposeAsync();
				scope.Dispose();
			}

			await Task.Delay(_delay, stoppingToken);
		}
	}

	private async Task SendGoalExpireNoti(
		IApplicationDbContext applicationDbContext,
		INotificationService notificationService,
		TenantDbContext tenantDbContext,
		ILoggerService loggerService)
	{
		try
		{
			var tenants = await tenantDbContext.Tenants.Where(s => s.DeleteFlag != true && s.ConnectionString != null).ToListAsync();

			DateTime time = DateTime.Now;
			DateTime expired = time.AddDays(2);

			var status = new List<string>()
			{
				GoalStatusEnum.COMPLETED.ToString(),
				GoalStatusEnum.FAILED.ToString(),
				GoalStatusEnum.PENDING.ToString()
			};

			List<PushNotificationRequest> notifications = new List<PushNotificationRequest>();
			foreach (var tenant in tenants)
			{
				applicationDbContext.SetConnectString(tenant.ConnectionString ?? "");

				var goals = applicationDbContext.Goals.Include(s => s.GoalStatus)
							.Where(g => g.DeleteFlag != true && g.EndTime != null && g.GoalStatus != null &&
										g.EndTime >= time && g.EndTime <= expired && g.SendExpiredNotification != true &&
										!status.Contains(g.GoalStatus!.Code!)
							)
							.ToList();

				if (goals.Any())
				{
					foreach (var goal in goals)
					{
						var noti = new PushNotificationRequest(
							NotificationAction.GoalMsg.Title,
							NotificationAction.GoalMsg.Expired(goal.EndTime!.Value),
							NotificationAction.GoalMsg.Type,
							goal.Id.ToString(),
							goal.UserSuggestId ?? null,
							tenant.Id
						);
						notifications.Add(noti);
						goal.SendExpiredNotification = true;
					}
					applicationDbContext.Goals.UpdateRange(goals);
					await applicationDbContext.SaveChangesAsync(new CancellationToken());
				}
            }
			if (notifications.Any())
			{
				await notificationService.PushAsync(notifications);
			}
        }
        catch (Exception ex)
		{
            loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
        }
	}
}
