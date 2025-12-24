using Sale_Saas.Application.Features.GoalFeature.Services;
using Sale_Saas.Application.Features.TargetFluctuationFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.TargetFluctuationFeature.Queries;

public record TargetFluctuation_GetByIdQuery(Guid UserId, Guid FluctuationId) : IRequest<Result<TargetFluctuationDto>>;

public class TargetFluctuation_GetByIdQueryHandler : IRequestHandler<TargetFluctuation_GetByIdQuery, Result<TargetFluctuationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IMapper _mapper;

    public TargetFluctuation_GetByIdQueryHandler(IApplicationDbContext context, IEventLogService eventLogService, IMapper mapper)
    {
        _context = context;
        _eventLogService = eventLogService;
        _mapper = mapper;
    }

    public async Task<Result<TargetFluctuationDto>> Handle(TargetFluctuation_GetByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        var item = await _context.TargetFluctuations.Where(x => x.DeleteFlag != true && x.Id == request.FluctuationId).AsNoTracking().FirstOrDefaultAsync();

        var result = _mapper.Map<TargetFluctuationDto>(item);

        result.CriteriaName = await GoalService.GetCriteriaName(result.GoalId ?? Guid.Empty, _context);

        return Result<TargetFluctuationDto>.Success(result);
    }
}
