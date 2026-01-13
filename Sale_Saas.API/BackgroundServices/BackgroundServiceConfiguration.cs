using Sale_Saas.Infrastructure.BackgroundServices;

namespace Sale_Saas.API.BackgroundServices;

public static class BackgroundServiceConfiguration
{
	public static IServiceCollection ConfigureBackground(this IServiceCollection services, BackgroundServiceSetting config)
	{
		if (config.DataExpiredScheduleEnabled == true) services.AddHostedService<DataExpiredBackgroundService>();
		if (config.TrackingLogScheduleEnabled == true) services.AddHostedService<TrackingLogBackgroundService>();
		if (config.DownTimeScheduleEnabled == true) services.AddHostedService<DownTimeBackgroundService>();
		if (config.SyllabusNotiBackgroundService == true) services.AddHostedService<SyllabusNotiBackgroundService>();

		return services;
	}
}
