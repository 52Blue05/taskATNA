using Sale_Saas.Application.Features.GainsFeature.RequestModels;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GainsFeature.Queries;

public record GainsMobile_GetByIdQuery(Guid userId, Guid Id) : IRequest<Result<GainsMobile_AddOrUpdateRequest>>;
public class GainsMobile_GetByIdQueryHandler : IRequestHandler<GainsMobile_GetByIdQuery, Result<GainsMobile_AddOrUpdateRequest>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IEventLogService _eventLogService;

    public GainsMobile_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _mapper = mapper;
        _eventLogService = eventLogService;
    }

    public async Task<Result<GainsMobile_AddOrUpdateRequest>> Handle(GainsMobile_GetByIdQuery request, CancellationToken cancellationToken)
    {
        var gains = await _context.Gains.Where(s => s.Id == request.Id)
                                        .Include(s => s.GainsFamilies.Where(gm => gm.DeleteFlag != true))
                                        .Include(s => s.GainsSchools.Where(gs => gs.DeleteFlag != true))
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();

        if (gains == null)
        {
            throw new ApplicationException("Không tìm thấy câu hỏi gains");
        }

        var data = _mapper.Map<GainsMobile_AddOrUpdateRequest>(gains);

        var eventLog = await _eventLogService.Create("GainsFeature", "GainsFeature",
                                "GainsMobile_GetByIdQuery", request.userId);

        return Result<GainsMobile_AddOrUpdateRequest>.Success(data);
    }
}
