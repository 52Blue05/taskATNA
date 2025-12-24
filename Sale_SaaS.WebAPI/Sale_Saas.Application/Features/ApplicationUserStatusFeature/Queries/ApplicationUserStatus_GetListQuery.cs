using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.ApplicationUserStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using System.Linq;


namespace Sale_Saas.Application.Features.ApplicationUserStatusFeature.Queries;

public record ApplicationUserStatus_GetListQuery(Guid userId, FilterQueryRequest RequestData) : IRequest<Result<IEnumerable<ApplicationUserStatusDto>>>;

public class ApplicationUserStatus_GetListQueryHandler : IRequestHandler<ApplicationUserStatus_GetListQuery, Result<IEnumerable<ApplicationUserStatusDto>>>
{		
    private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper;
	private readonly IEventLogService _eventLogService;

	public ApplicationUserStatus_GetListQueryHandler(IMapper mapper, 
                                    IApplicationDbContext context, IEventLogService eventLogService)		
	{
        _context = context;
		_mapper = mapper;
		_eventLogService = eventLogService;
	}

    public async Task<Result<IEnumerable<ApplicationUserStatusDto>>> Handle(ApplicationUserStatus_GetListQuery request, CancellationToken cancellationToken)
    {

        var query = from cv in _context.ApplicationUserStatuses
					where cv.DeleteFlag != true
					select new ApplicationUserStatusDto()
					{
						Id = cv.Id,
						Code = cv.Code ?? string.Empty,
						Name = cv.Name ?? string.Empty,
					};

		if (request.RequestData.Skip != null)
		{
			query = query.Skip(request.RequestData.Skip.Value);
		}

		if (request.RequestData.TotalRecord != null)
		{
			query = query.Take(request.RequestData.TotalRecord.Value);
		}

        var eventLog = await _eventLogService.Create("ApplicationUserStatusFeature", "ApplicationUserStatusFeature",
            "ApplicationUserStatus_GetListQuery", request.userId);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<IEnumerable<ApplicationUserStatusDto>>.Success((await query.AsNoTracking()
																				  .ToListAsync())
																				  .AsReadOnly());
    }
}
