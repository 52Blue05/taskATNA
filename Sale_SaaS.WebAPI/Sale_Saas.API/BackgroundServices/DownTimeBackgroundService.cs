using Microsoft.EntityFrameworkCore;
using Sale_Saas.Application.Common.Interfaces;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Infrastructure.Data;
using System.Reflection;


namespace Sale_Saas.API.BackgroundServices;

public class DownTimeBackgroundService : BackgroundService
{
	private readonly TimeSpan _delay = TimeSpan.FromMinutes(1);
	private readonly IServiceScopeFactory _serviceProvider;

	public DownTimeBackgroundService(IServiceScopeFactory serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(_delay, stoppingToken);

			using (var scope = _serviceProvider.CreateScope())
			{
				var serviceProvider = scope.ServiceProvider;
				var context = serviceProvider.GetRequiredService<TenantDbContext>();
				var apcontext = serviceProvider.GetRequiredService<IApplicationDbContext>();
				var userService = serviceProvider.GetRequiredService<IApplicationUserService>();
				var loggerService = serviceProvider.GetRequiredService<ILoggerService>();

				await DownTimeServer(context, loggerService);
				await DownTimeDb(context, apcontext, userService, loggerService);

				await context.DisposeAsync();
				await apcontext.DisposeAsync();
				scope.Dispose();
			}
		}
	}

	private async Task DownTimeServer(TenantDbContext _context, ILoggerService _loggerService)
	{
		try
		{
			var lastTrackingLog = await _context.TrackingLogs.OrderByDescending(tl => tl.LastModifiedDate)
											 .Where(tl => tl.Type == "TRASH SERVER")
											 .FirstOrDefaultAsync();

			//var tenants = await _context.Tenants.Where(s => s.DeleteFlag != true && s.ConnectionString != null).ToListAsync();

			if (lastTrackingLog != null)
			{
				float totalDowtimeSed = (float)(DateTime.Now - lastTrackingLog.LastModifiedDate).TotalSeconds;
				TrackingLog trackingLog = new TrackingLog
				{
					Id = Guid.NewGuid(),
					ResponseTimeSec = totalDowtimeSed,
					ResponseTimeMin = totalDowtimeSed / 60,
					Type = "DOWNTIME SERVER",
					Message = "DOWNTIME SERVER",
                    DeleteFlag = false,
					CreatedDate = DateTime.Now,
					LastModifiedDate = DateTime.Now
				};
				if (totalDowtimeSed <= 90)
				{
					trackingLog.Type = "TRASH SERVER";

				}
				if (lastTrackingLog.Type == "TRASH SERVER")
				{
					var removeTracking = await _context.TrackingLogs.Where(tl => tl.Type == "TRASH SERVER").ToListAsync();
					_context.TrackingLogs.RemoveRange(removeTracking);
				}

				_context.TrackingLogs.Add(trackingLog);

				await _context.SaveChangesAsync();
			}
			else
			{
				TrackingLog trackingLog = new TrackingLog
				{
					Id = Guid.NewGuid(),
					ResponseTimeSec = 0,
					ResponseTimeMin = 0,
					Type = "TRASH SERVER",
					DeleteFlag = false,
					CreatedDate = DateTime.Now,
					LastModifiedDate = DateTime.Now
				};
				_context.TrackingLogs.Add(trackingLog);

				await _context.SaveChangesAsync();
			}
		}
		catch (Exception ex) 
		{
            _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
        }
	}

	private async Task DownTimeDb(TenantDbContext _context,
										IApplicationDbContext _apcontext,
										IApplicationUserService _userService,
										ILoggerService _loggerService)
	{
		try
		{
			//var tenants = await _context.Tenants.Where(s => s.DeleteFlag != true && s.ConnectionString != null).ToListAsync();
			var lastTenant = await _context.TrackingLogs.OrderByDescending(tl => tl.LastModifiedDate)
						 .Where(tl => (tl.Type == "TRASH DATABASE") && tl.TenantId == null)
							.FirstOrDefaultAsync();

			if (lastTenant != null)
			{
				float totalDowtimeSed = (float)(DateTime.Now - lastTenant.LastModifiedDate).TotalSeconds;
				TrackingLog trackingLog = new TrackingLog
				{
					Id = Guid.NewGuid(),
					ResponseTimeSec = totalDowtimeSed,
					ResponseTimeMin = totalDowtimeSed / 60,
					Type = "DOWNTIME DATABASE",
					DeleteFlag = false,
					Message = "DOWNTIME DATABASE",
                    CreatedDate = DateTime.Now,
					LastModifiedDate = DateTime.Now,
					TenantId = null
				};
				if (totalDowtimeSed <= 90)
				{
					trackingLog.Type = "TRASH DATABASE";

				}
				if (lastTenant.Type == "TRASH DATABASE")
				{
					var removeTracking = await _context.TrackingLogs.Where(tl => tl.Type == "TRASH DATABASE" && tl.TenantId == null).ToListAsync();
					_context.TrackingLogs.RemoveRange(removeTracking);
				}

				_context.TrackingLogs.Add(trackingLog);

				await _context.SaveChangesAsync();
			}
			else
			{
				TrackingLog trackingLog = new TrackingLog
				{
					Id = Guid.NewGuid(),
					ResponseTimeSec = 0,
					ResponseTimeMin = 0,
					Type = "TRASH DATABASE",
					DeleteFlag = false,
					CreatedDate = DateTime.Now,
					LastModifiedDate = DateTime.Now,
					TenantId = null
				};
				_context.TrackingLogs.Add(trackingLog);

				await _context.SaveChangesAsync();
			}

			var listTenant = await _context.Tenants.ToListAsync();
			foreach (var tenant in listTenant)
			{
				try
				{
					_userService.SetConnectDB(tenant.ConnectionString ?? "");
					var checkTenant = await _apcontext.ApplicationUsers.FirstOrDefaultAsync();
					_userService.ClearConnectDB();
                    var lastTrackingLog = await _context.TrackingLogs.OrderByDescending(tl => tl.LastModifiedDate)
								 .Where(tl => (tl.Type == "TRASH DATABASE") && tl.TenantId == tenant.Id)
								 .FirstOrDefaultAsync();
					if (lastTrackingLog != null)
					{
						float totalDowtimeSed = (float)(DateTime.Now - lastTrackingLog.LastModifiedDate).TotalSeconds;
						TrackingLog trackingLog = new TrackingLog
						{
							Id = Guid.NewGuid(),
							ResponseTimeSec = totalDowtimeSed,
							ResponseTimeMin = totalDowtimeSed / 60,
							Type = "DOWNTIME DATABASE",
							DeleteFlag = false,
							CreatedDate = DateTime.Now,
							LastModifiedDate = DateTime.Now,
							TenantId = tenant.Id
						};
						if (totalDowtimeSed <= 90)
						{
							trackingLog.Type = "TRASH DATABASE";

						}
						if (lastTrackingLog.Type == "TRASH DATABASE")
						{
							var removeTracking = await _context.TrackingLogs.Where(tl => tl.Type == "TRASH DATABASE" && tl.TenantId == tenant.Id).ToListAsync();
							_context.TrackingLogs.RemoveRange(removeTracking);
						}

						_context.TrackingLogs.Add(trackingLog);

						await _context.SaveChangesAsync();
					}
					else
					{
						TrackingLog trackingLog = new TrackingLog
						{
							Id = Guid.NewGuid(),
							ResponseTimeSec = 0,
							ResponseTimeMin = 0,
							Type = "TRASH DATABASE",
							DeleteFlag = false,
							CreatedDate = DateTime.Now,
							LastModifiedDate = DateTime.Now,
							TenantId = tenant.Id
						};
						_context.TrackingLogs.Add(trackingLog);

						await _context.SaveChangesAsync();
					}
				}
				catch (Exception ex) 
				{
                    _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                }
			}
		}
		catch (Exception ex) 
		{
            _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
        }
	}
}
