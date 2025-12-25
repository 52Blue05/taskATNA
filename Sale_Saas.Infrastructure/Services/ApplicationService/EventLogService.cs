using Sale_Saas.Application.Common.Interfaces;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Infrastructure.Services.ApplicationService;

public class EventLogService : IEventLogService
{
	private readonly IApplicationDbContext _context;
	public EventLogService(IApplicationDbContext context)
	{
		_context = context;
	}
	
	public async Task<EventLog> Create(string code, string name, string action, Guid? user = null)
	{
		try
		{
			var eventLog = new EventLog()
			{
				Id = Guid.NewGuid(),
				Code = code,
				Name = name,
				Action = action,
				CreatedDate = DateTime.Now,
				LastModifiedDate = DateTime.Now,
				CreatedApplicationUserId = user,
				LastModifiedApplicationUserId = user
			};
			_context.EventLogs.Add(eventLog);
			await _context.SaveChangesAsync(new CancellationToken());
			return eventLog;
		}
		catch(Exception ex) {
			return new EventLog();
		}
	}

}
