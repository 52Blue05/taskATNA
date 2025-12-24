using Sale_Saas.Application.Features.GoalFeature.Dto;
using Sale_Saas.Application.Features.GoalFeature.Services;
using Sale_Saas.Application.Features.GoalStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Models.Identity;
namespace Sale_Saas.Application.Features.GoalFeature.Queries;

public record Goal_GetAllQuery(Guid userId, GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<GoalDto>>>;
public class Goal_GetAllQueryHandler : IRequestHandler<Goal_GetAllQuery, Result<IEnumerable<GoalDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IEventLogService _eventLogService;

    public Goal_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _mapper = mapper;
        _eventLogService = eventLogService;
    }

    public async Task<Result<IEnumerable<GoalDto>>> Handle(Goal_GetAllQuery request, CancellationToken cancellationToken)
    {
        await GoalService.SyncData(_context, cancellationToken);
        IEnumerable<GoalDto> KPIs = (await (from cv in _context.Goals
                                            join user in _context.ApplicationUsers on cv.UserSuggestId equals user.Id
                                            join status in _context.GoalStatuses on cv.GoalStatusId equals status.Id
                                            join role in _context.ApplicationRoles on cv.ApplicationRoleId equals role.Id into role_cv
                                            from role in role_cv.DefaultIfEmpty()

                                            where cv.DeleteFlag != true && user.DeleteFlag != true && (role != null && role.DeleteFlag != true)
                                            orderby cv.CreatedDate descending
                                            select new GoalDto()
                                            {
                                                Id = cv.Id,
                                                CriteriaName = cv.CriteriaName ?? "",
                                                TargetKPI = cv.TargetKPI ?? 0,
                                                ActualKPI = cv.ActualKPI,
                                                TargetPoint = cv.TargetPoint ?? 0,
                                                ActualPoint = cv.ActualPoint,
                                                SuggestTargetKPI = cv.SuggestTargetKPI ?? 0,
                                                SuggestTargetPoint = cv.SuggestTargetPoint ?? 0,
                                                SuggestEndTime = cv.SuggestEndTime ?? new DateTime(),
                                                Calculate = cv.Calculate ?? "",
                                                StartTime = cv.StartTime ?? new DateTime(),
                                                EndTime = cv.EndTime ?? new DateTime(),
                                                Review = cv.Review ?? "",
                                                UserSuggest = new UserBasicInfoDto
                                                {
                                                    Id = user.Id,
                                                    FirstName = user.FirstName ?? "",
                                                    LastName = user.LastName ?? "",
                                                    Email = user.Email ?? "",
                                                    Phone = user.PhoneNumber ?? "",
                                                    FullName = user.FullName ?? ""
                                                },
                                                GoalStatus = new GoalStatusDto
                                                {
                                                    Id = status.Id,
                                                    Code = status.Code ?? "",
                                                    Name = status.Name ?? ""
                                                },
                                                ApplicationRole = role != null ? new ApplicationRoleDto
                                                {
                                                    Id = role.Id,
                                                    Name = role.Name ?? "",
                                                    DisplayName = role.DisplayName ?? "",
                                                    Description = role.Description ?? "",
                                                    RolePositionId = role.RolePositionId ?? ""
                                                } : null
                                            }).AsNoTracking().ToListAsync()).AsReadOnly();

        var eventLog = await _eventLogService.Create("GoalFeature", "GoalFeature",
                                                "Goal_GetAllQuery", request.userId);

        return Result<IEnumerable<GoalDto>>.Success(KPIs);
    }
}
