using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Infrastructure.Data;
using Sale_Saas.Infrastructure.Services.Identity;
using System.Reflection;

namespace Sale_Saas.Infrastructure.BackgroundServices;

public class TrackingLogBackgroundService : BackgroundService
{
	private readonly IServiceScopeFactory _serviceProvider;
	private readonly TimeSpan _delay = TimeSpan.FromMinutes(2);

	public TrackingLogBackgroundService(IServiceScopeFactory serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(_delay, stoppingToken);

			while (!TrackingLogQueue.IsEmpty())
			{
				var logEntry = TrackingLogQueue.Dequeue();
				if (logEntry != null)
				{
					using (var scope = _serviceProvider.CreateScope())
					{
						var dbContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
						var loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();
						dbContext.TrackingLogs.Add(logEntry);

						try
						{
							await dbContext.SaveChangesAsync();
							await dbContext.DisposeAsync();
							scope.Dispose();                        }
                        catch (Exception ex)
						{
                            loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                        }
					}
				}
			}
		}
	}
}
