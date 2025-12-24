using Sale_Saas.Application.Features.GoalFeature.Dto;
using Sale_Saas.Application.Features.GoalFeature.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GoalFeature.Queries;

public record Goal_GetListQuery(Guid userId, FilterQueryRequest RequestData) : IRequest<Result<IEnumerable<GoalDto>>>;

public class Goal_GetListQueryHandler : IRequestHandler<Goal_GetListQuery, Result<IEnumerable<GoalDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IEventLogService _eventLogService;

    public Goal_GetListQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _mapper = mapper;
        _eventLogService = eventLogService;
    }

    public async Task<Result<IEnumerable<GoalDto>>> Handle(Goal_GetListQuery request, CancellationToken cancellationToken)
    {
        await GoalService.SyncData(_context, cancellationToken);

        var query = _context.Goals.Where(s => s.DeleteFlag != true && s.UserSuggest != null && s.UserSuggest.DeleteFlag != true && s.ApplicationRole != null && s.ApplicationRole.DeleteFlag != true)
                                        .Include(s => s.UserSuggest)
                                        .Include(s => s.GoalStatus)
                                        .Include(s => s.ApplicationRole)
                                        .OrderByDescending(x => x.CreatedDate)
                                        .ProjectTo<GoalDto>(_mapper.ConfigurationProvider)
                                        .AsNoTracking();

        if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
        {
            query = query.Where(x => (x.CriteriaName != null && x.CriteriaName.ToLower().Contains(request.RequestData.TextSearch.ToLower())) ||
                                     (x.UserSuggest!.FirstName != null && x.UserSuggest!.FirstName.ToLower().Contains(request.RequestData.TextSearch.ToLower())) ||
                                     (x.UserSuggest!.LastName != null && x.UserSuggest!.LastName.ToLower().Contains(request.RequestData.TextSearch.ToLower())) ||
                                     (x.UserSuggest!.LastName != null && x.UserSuggest!.LastName.ToLower().Contains(request.RequestData.TextSearch.ToLower())));
        }

        if (request.RequestData.Skip != null)
        {
            query = query.Skip(request.RequestData.Skip.Value);
        }

        if (request.RequestData.TotalRecord != null)
        {
            query = query.Take(request.RequestData.TotalRecord.Value);
        }

        var data = await query.AsNoTracking().ToListAsync();

        var eventLog = await _eventLogService.Create("GoalFeature", "GoalFeature",
                                                "Goal_GetListQuery", request.userId);

        return Result<IEnumerable<GoalDto>>.Success(data);
    }
}
