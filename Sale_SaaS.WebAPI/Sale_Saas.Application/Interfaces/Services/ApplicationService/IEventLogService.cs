namespace Sale_Saas.Application.Interfaces.Services.ApplicationService;

public interface IEventLogService
{
	Task<EventLog> Create(string code, string name, string action, Guid? user = null);
}
