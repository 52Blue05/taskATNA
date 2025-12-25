using Sale_Saas.Application.Features.GoalStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GoalStatusFeature.Queries
{
    public record GoalStatus_GetByIdQuery(Guid userId, Guid Id) : IRequest<Result<GoalStatusDto>>;
    public class GoalStatus_GetByIdQueryHandler : IRequestHandler<GoalStatus_GetByIdQuery, Result<GoalStatusDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public GoalStatus_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<GoalStatusDto>> Handle(GoalStatus_GetByIdQuery request, CancellationToken cancellationToken)
        {
            GoalStatusDto? GoalStatus = await (from cv in _context.GoalStatuses
                                                           where cv.DeleteFlag != true
                                                           select new GoalStatusDto()
                                                           {
                                                               Id = cv.Id,
                                                               Code = cv.Code ?? "",
                                                               Name = cv.Name ?? ""
                                                           }).AsNoTracking().FirstOrDefaultAsync();

            var eventLog = await _eventLogService.Create("GoalStatusFeature", "GoalStatusFeature",
                                    "GoalStatus_GetByIdQuery", request.userId);
            return Result<GoalStatusDto>.Success(GoalStatus);
        }
    }
}
