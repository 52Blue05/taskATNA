using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.ApplicationUserStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;


namespace Sale_Saas.Application.Features.ApplicationUserStatusFeature.Queries;

public record ApplicationUserStatus_GetAllQuery(Guid userId, GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<ApplicationUserStatusDto>>>;

public class ApplicationUserStatus_GetAllQueryHandler : IRequestHandler<ApplicationUserStatus_GetAllQuery, Result<IEnumerable<ApplicationUserStatusDto>>>
{		
    private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper;
    private readonly IEventLogService _eventLogService;

	public ApplicationUserStatus_GetAllQueryHandler(IMapper mapper, 
                                    IApplicationDbContext context, IEventLogService eventLogService)		
	{
        _context = context;
		_mapper = mapper;
        _eventLogService = eventLogService;
	}

    public async Task<Result<IEnumerable<ApplicationUserStatusDto>>> Handle(ApplicationUserStatus_GetAllQuery request, CancellationToken cancellationToken)
    {
        var eventLog = await _eventLogService.Create("ApplicationUserStatusFeature", "ApplicationUserStatusFeature",
                                "ApplicationUserStatus_GetAllQuery", request.userId);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<IEnumerable<ApplicationUserStatusDto>>.Success((await (from cv in _context.ApplicationUserStatuses
                      where cv.DeleteFlag != true
                      select new ApplicationUserStatusDto()
                      {
                          Id = cv.Id,
                          Code = cv.Code ?? string.Empty,
                          Name = cv.Name ?? string.Empty,
                      })
                      .AsNoTracking()
                      .ToListAsync())
                      .AsReadOnly());
    }
}
