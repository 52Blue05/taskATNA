using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.BenefitHistoryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.BenefitHistoryFeature.Queries;

public record BenefitHistory_GetListWithPaginationQuery(Guid userId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<BenefitHistoryDto>>>;

public class BenefitHistory_GetListWithPaginationQueryHandler : IRequestHandler<BenefitHistory_GetListWithPaginationQuery, Result<PaginatedList<BenefitHistoryDto>>>
{
	private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper;
    private readonly IEventLogService _eventLogService;

    public BenefitHistory_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
	{
		_context = context;
		_mapper = mapper;
		_eventLogService = eventLogService;
	}

	public async Task<Result<PaginatedList<BenefitHistoryDto>>> Handle(BenefitHistory_GetListWithPaginationQuery request, CancellationToken cancellationToken)
	{
		var query = _context.BenefitHistories.Include(s => s.ApplicationUser).Include(s => s.PreviousStatus).Include(s => s.UpdatedStatus)
							.Where(m => m.DeleteFlag != true)
							.OrderByDescending(x => x.CreatedDate)
							.ProjectTo<BenefitHistoryDto>(_mapper.ConfigurationProvider)
							.AsNoTracking();

		if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
		{
			query = query.Where(s => s.ApplicationUser.ToLower().Contains(request.RequestData.TextSearch.ToLower()));
		}

        var eventLog = await _eventLogService.Create("BenefitHistoryFeature", "BenefitHistoryFeature","BenefitHistory_GetListWithPaginationQuery", request.userId);

        return Result<PaginatedList<BenefitHistoryDto>>.Success(await query.PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize));
	}
}
