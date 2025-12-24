using Sale_Saas.Application.Features.GoalStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GoalStatusFeature.Queries
{
    public record GoalStatus_GetAllQuery(Guid userId, GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<GoalStatusDto>>>;
    public class GoalStatus_GetAllQueryHandler : IRequestHandler<GoalStatus_GetAllQuery, Result<IEnumerable<GoalStatusDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public GoalStatus_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<IEnumerable<GoalStatusDto>>> Handle(GoalStatus_GetAllQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<GoalStatusDto> statuses = (await (from cv in _context.GoalStatuses
                                                                where cv.DeleteFlag != true
                                                                select new GoalStatusDto()
                                                                {
                                                                    Id = cv.Id,
                                                                    Code = cv.Code ?? "",
                                                                    Name = cv.Name ?? ""
                                                                }).AsNoTracking().ToListAsync()).AsReadOnly();

            var eventLog = await _eventLogService.Create("GoalStatusFeature", "GoalStatusFeature",
                                    "GoalStatus_GetAllQuery", request.userId);
            return Result<IEnumerable<GoalStatusDto>>.Success(statuses);
        }
    }
}
