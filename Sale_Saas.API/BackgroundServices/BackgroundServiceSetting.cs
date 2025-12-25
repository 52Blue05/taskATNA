namespace Sale_Saas.API.BackgroundServices;	

public class BackgroundServiceSetting
{	
	public bool DownTimeScheduleEnabled { get; set; }
	public bool TrackingLogScheduleEnabled { get; set; }
	public bool DataExpiredScheduleEnabled { get; set; }
	public bool SyllabusNotiBackgroundService {  get; set; }
}
