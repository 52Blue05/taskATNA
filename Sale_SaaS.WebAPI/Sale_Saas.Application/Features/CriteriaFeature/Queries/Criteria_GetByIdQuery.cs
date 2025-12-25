using Sale_Saas.Application.Features.CriteriaFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.CriteriaFeature.Queries;

public record Criteria_GetByIdQuery(Guid UserId, Guid Id) : IRequest<Result<CriteriaDto>>;

public class Criteria_GetByIdQueryHandler : IRequestHandler<Criteria_GetByIdQuery, Result<CriteriaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IMapper _mapper;

    public Criteria_GetByIdQueryHandler(IApplicationDbContext context, IEventLogService eventLogService, IMapper mapper)
    {
        _context = context;
        _eventLogService = eventLogService;
        _mapper = mapper;
    }

    public async Task<Result<CriteriaDto>> Handle(Criteria_GetByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy người dùng");
        }

        var criteria = await _context.Criterias.Where(x => x.DeleteFlag != true && x.Id == request.Id)
                                               .AsNoTracking()
                                               .FirstOrDefaultAsync();

        if (criteria == null)
        {
            throw new ApplicationException("Không tìm thấy tiêu chí");
        }

        var criteriaDto = _mapper.Map<CriteriaDto>(criteria);

        var eventLog = await _eventLogService.Create("CriteriaFeature", "CriteriaFeature", "Criteria_GetByIdQuery", request.Id);

        return Result<CriteriaDto>.Success(criteriaDto);
    }
}
