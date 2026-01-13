using Sale_Saas.Application.Features.GoalFeature.Services;
using Sale_Saas.Application.Features.TargetFluctuationFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.TargetFluctuationFeature.Queries;

public record TargetFluctuation_GetListWithPaginationQuery(Guid UserId, Guid BenefitId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<TargetFluctuationDto>>>;

public class TargetFluctuation_GetListWithPaginationQueryHandler : IRequestHandler<TargetFluctuation_GetListWithPaginationQuery, Result<PaginatedList<TargetFluctuationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IMapper _mapper;

    public TargetFluctuation_GetListWithPaginationQueryHandler(IApplicationDbContext context, IEventLogService eventLogService, IMapper mapper)
    {
        _context = context;
        _eventLogService = eventLogService;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<TargetFluctuationDto>>> Handle(TargetFluctuation_GetListWithPaginationQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        if (request.BenefitId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy quyền lợi hiện tại");
        }

        var listTargetFluctuation = await _context.TargetFluctuations.Where(x => x.DeleteFlag != true && x.BenefitId == request.BenefitId).AsNoTracking().ToListAsync();

        var result = _mapper.Map<List<TargetFluctuationDto>>(listTargetFluctuation);

        foreach (var item in result)
        {
            item.CriteriaName = await GoalService.GetCriteriaName(item.GoalId ?? Guid.Empty, _context);
        }

        return Result<PaginatedList<TargetFluctuationDto>>.Success(new PaginatedList<TargetFluctuationDto>(result, result.Count, request.RequestData.PageIndex, request.RequestData.PageSize));
    }
}