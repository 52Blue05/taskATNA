using Sale_Saas.Application.Features.GoalStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GoalStatusFeature.Queries
{
    public record GoalStatus_GetByNameQuery(Guid userId, string Name) : IRequest<Result<GoalStatusDto>>;
    public class GoalStatus_GetByNameQueryHandler : IRequestHandler<GoalStatus_GetByNameQuery, Result<GoalStatusDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public GoalStatus_GetByNameQueryHandler(IApplicationDbContext context, IMapper mapper, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<GoalStatusDto>> Handle(GoalStatus_GetByNameQuery request, CancellationToken cancellationToken)
        {
            GoalStatusDto? GoalStatus = await (from goal in _context.GoalStatuses
                                                       where goal.DeleteFlag != true && goal.Name == request.Name
                                                       select new GoalStatusDto()
                                                       {
                                                           Id = goal.Id,
                                                           Code = goal.Code ?? "",
                                                           Name = goal.Name ?? ""
                                                       }).AsNoTracking().FirstOrDefaultAsync();

            var eventLog = await _eventLogService.Create("GoalStatusFeature", "GoalStatusFeature",
                                    "GoalStatus_GetByNameQuery", request.userId);
            return Result<GoalStatusDto>.Success(GoalStatus);
        }
    }
}
