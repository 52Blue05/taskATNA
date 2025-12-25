using Sale_Saas.Application.Features.GoalFeature.Dto;
using Sale_Saas.Application.Features.GoalFeature.Services;
using Sale_Saas.Application.Features.GoalStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Features.GoalFeature.Queries;

public record GoalMobile_GetByIdQuery(Guid userId, Guid Id) : IRequest<Result<GoalDto>>;
public class GoalMobile_GetByIdQueryHandler : IRequestHandler<GoalMobile_GetByIdQuery, Result<GoalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IEventLogService _eventLogService;

    public GoalMobile_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _mapper = mapper;
        _eventLogService = eventLogService;
    }

    public async Task<Result<GoalDto>> Handle(GoalMobile_GetByIdQuery request, CancellationToken cancellationToken)
    {
        await GoalService.SyncData(_context, cancellationToken);

        GoalDto? Goal = await (from cv in _context.Goals
                               join user in _context.ApplicationUsers on cv.UserSuggestId equals user.Id
                               join status in _context.GoalStatuses on cv.GoalStatusId equals status.Id
                               join role in _context.ApplicationRoles on cv.ApplicationRoleId equals role.Id into role_cv
                               from role in role_cv.DefaultIfEmpty()
                               where cv.DeleteFlag != true && cv.Id == request.Id && user.DeleteFlag != true && (role != null && role.DeleteFlag != true)
                               select new GoalDto()
                               {
                                   Id = cv.Id,
                                   CriteriaName = cv.CriteriaName ?? "",
                                   CriteriaType = cv.CriteriaType ?? "",
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
                                   UserSuggest = user.DeleteFlag == true ? new UserBasicInfoDto() : new UserBasicInfoDto
                                   {
                                       Id = user.Id,
                                       FirstName = user.FirstName ?? "",
                                       LastName = user.LastName ?? "",
                                       Email = user.Email ?? "",
                                       Phone = user.PhoneNumber ?? "",
                                       FullName = user.FullName ?? ""
                                   },
                                   GoalStatus = status.DeleteFlag == true ? new GoalStatusDto() : new GoalStatusDto
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
                               }).AsNoTracking().FirstOrDefaultAsync();
        var listCustomer = await _context.Relationships.Where(x => x.GoalId == Goal.Id).Select(x => new CustomerByGoal()
        {
            Id = x.Id,
            FullName = x.CustomerName,
            Point = x.Point,
            ActualPoint = x.ActualPoint,
        }).ToListAsync();
        Goal.Customers = listCustomer;
        Goal.TotalCustomer = listCustomer.Count;
        var eventLog = await _eventLogService.Create("GoalFeature", "GoalFeature",
                                                "GoalMobile_GetByIdQuery", request.userId);
        return Result<GoalDto>.Success(Goal);
    }
}
