using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.GoalFeature.Dto;
using Sale_Saas.Application.Features.GoalFeature.Requests;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Enums;
using System.Linq;

namespace Sale_Saas.Application.Features.GoalFeature.Queries;

public record GoalMobile_GetListWithPaginationQuery(Guid userId, GetListWithPaginationQueryRequest RequestData, GetListGoalMobileRequest Body) : IRequest<Result<GoalPaginatedList<GoalDto>>>;

public class GoalMobile_GetListWithPaginationQueryHandler : IRequestHandler<GoalMobile_GetListWithPaginationQuery, Result<GoalPaginatedList<GoalDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFeaturePermissionService _permissionService;
    private readonly IApplicationUserService _userService;
    private readonly IEventLogService _eventLogService;

    public GoalMobile_GetListWithPaginationQueryHandler(
        IMapper mapper, IApplicationDbContext context, IFeaturePermissionService permissionService,
        IApplicationUserService userService, IEventLogService eventLogService)
    {
        _context = context;
        _mapper = mapper;
        _permissionService = permissionService;
        _userService = userService;
        _eventLogService = eventLogService;
    }

    public async Task<Result<GoalPaginatedList<GoalDto>>> Handle(GoalMobile_GetListWithPaginationQuery request, CancellationToken cancellationToken)
    {
        //await GoalService.SyncData(_context, cancellationToken);
        await InitialCriteria(cancellationToken);

        int totalTargetPoint = 0;
        var query = _context.Goals.Where(m => m.DeleteFlag != true
                            && m.UserSuggest != null && m.UserSuggest.DeleteFlag != true && m.ApplicationRole != null && m.ApplicationRole.DeleteFlag != true)
                                  .Include(s => s.GoalStatus)
                                  .Include(s => s.UserSuggest)
                                  .Include(s => s.ApplicationRole)
                                  .Include(s => s.Criteria)
                                  .OrderByDescending(x => x.CreatedDate)
                                  .ProjectTo<GoalDto>(_mapper.ConfigurationProvider)
                                  .AsNoTracking();

        if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
        {
            query = query.Where(s => s.CriteriaName.ToLower().Contains(request.RequestData.TextSearch.ToLower()) ||
                                     s.UserSuggest.FullName.ToLower().Contains(request.RequestData.TextSearch.ToLower()));
        }
        if (request.RequestData.RoleId != null)
        {
            query = query.Where(s => s.ApplicationRoleId == request.RequestData.RoleId);
        }
        if (request.RequestData.RoleType != null)
        {
            var user = request.userId;//User login
            if (request.RequestData.RoleType == RoleType.MYSELF.ToString())
            {
                if (request.RequestData.UserId == null)
                {
                    throw new ApplicationException("Không tìm thấy người dùng");
                }
                await _permissionService.HasPermission(
                    MenuType.Sale_MT,
                    FeatureType.MYSELF,
                    user,
                    true
                );
                query = query.Where(s => s.UserSuggest.Id == request.RequestData.UserId);

                var queryListGoal = query.Where(x => x.GoalStatus.Code != GoalStatusEnum.PENDING.ToString());

                totalTargetPoint = (int)queryListGoal.Sum(x => x.TargetPoint);
            }
            else if (request.RequestData.RoleType == RoleType.MANAGER.ToString())
            {
                await _permissionService.HasPermission(
                    MenuType.Sale_MT,
                    FeatureType.MANAGER,
                    user,
                    true
                );
                var ids = await _userService.GetUserIdByRolePosition(RolePositionEnum.MANAGER.ToString());
                query = query.Where(s => ids.Contains(s.UserSuggest.Id));

                query = query.Where(s => s.ApplicationRole.RolePositionId == RolePositionEnum.MANAGER.ToString());
                if (request.RequestData.UserId != null)
                {
                    query = query.Where(s => s.UserSuggest.Id == request.RequestData.UserId);
                }
            }
            else if (request.RequestData.RoleType == RoleType.EMPLOYEE.ToString())
            {
                await _permissionService.HasPermission(
                    MenuType.Sale_MT,
                    FeatureType.EMPLOYEE,
                    user,
                    true
                );
                var ids = await _userService.GetUserIdByRolePosition(RolePositionEnum.EMPLOYEE.ToString());
                query = query.Where(s => ids.Contains(s.UserSuggest.Id));

                query = query.Where(s => s.ApplicationRole.RolePositionId == RolePositionEnum.EMPLOYEE.ToString());
                if (request.RequestData.UserId != null)
                {
                    query = query.Where(s => s.UserSuggest.Id == request.RequestData.UserId);
                }
                query = query.Where(s => s.UserSuggest.Id != request.userId);

                var queryListGoal = query.Where(x => x.GoalStatus.Code != GoalStatusEnum.PENDING.ToString());

                totalTargetPoint = (int)queryListGoal.Sum(x => x.TargetPoint);
            }
            else
            {
                throw new ApplicationException($"Không tìm thấy dữ liệu {request.RequestData.RoleType}");
            }

            var position = await _permissionService.GetPositionByUser(user);
            if (position == RolePositionEnum.ADMINISTRATOR.ToString())
            {
                //query = query.Where(s => s.GoalStatus.Code != GoalStatusEnum.FAILED.ToString());
            }
        }

        //if (request.RequestData.StatusId != null)
        //{
        //    query = query.Where(s => s.GoalStatus.Id == request.RequestData.StatusId);
        //}
        if (request.Body.StatusIds != null && request.Body.StatusIds.Count > 0)
        {
            query = query.Where(s => request.Body.StatusIds.Contains(s.GoalStatus.Id));
            query = query.OrderBy(s => s.GoalStatus.Id);
        }
        else
        {
            if (request.RequestData.StatusId != null)
            {
                query = query.Where(s => s.GoalStatus.Id == request.RequestData.StatusId);
            }
        }

        if (request.RequestData.Time != null)
        {
            query = query.Where(s => s.StartTime.Year == request.RequestData.Time.Value.Year);
        }
        if (request.RequestData.BenefitId != null)
        {
            query = query.Where(s => s.BenefitId == request.RequestData.BenefitId);
        }
        if (request.RequestData.FromDate != null)
        {
            query = query.Where(s => s.StartTime >= request.RequestData.FromDate);
        }
        if (request.RequestData.ToDate != null)
        {
            query = query.Where(s => s.EndTime <= request.RequestData.ToDate);
        }
       

        query = query.OrderByDescending(s => s.CreatedDate);

        //int totalKpi = (int)query.Where(s => s.ActualPoint != null).Sum(s => s.ActualPoint);
        int totalActualPoint = (int)query.Sum(s => s.ActualPoint ?? 0);

        GoalPaginatedList<GoalDto> paging = await query.GoalPaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize);
        paging.TotalExtend = totalActualPoint;
        paging.TotalActualPoint = totalActualPoint;
        paging.TotalTargetPoint = totalTargetPoint;
        foreach (var item in paging.Items)
        {
            if (item.CriteriaType == CriteriaEnum.CRITERIA_CUSTOMER.ToString())
            {
                var listCustomer = await _context.Relationships.Where(x => x.GoalId == item.Id && x.DeleteFlag != true)
                                                               .AsNoTracking()
                                                               .Select(x => new CustomerByGoal()
                                                               {
                                                                   Id = x.Id,
                                                                   FullName = x.CustomerName,
                                                                   Point = x.Point ?? 0,
                                                                   ActualPoint = x.ActualPoint ?? 0,
                                                               })
                                                               .ToListAsync();
                item.Customers = listCustomer;
                item.TotalCustomer = listCustomer.Count;
            }
        }
        var eventLog = await _eventLogService.Create("GoalFeature", "GoalFeature",
                                                "GoalMobile_GetListWithPaginationQuery", request.userId);

        return Result<GoalPaginatedList<GoalDto>>.Success(paging);
    }

    private async Task InitialCriteria(CancellationToken cancellationToken)
    {
        if (!_context.Criterias.Any())
        {
            var criteria1 = new Criteria()
            {
                Id = Guid.NewGuid(),
                Code = "CRITERIA_CUSTOMER",
                Name = "Phát triển quan hệ khách hàng"
            };

            _context.Criterias.Add(criteria1);

            var criteria2 = new Criteria()
            {
                Id = Guid.NewGuid(),
                Code = "CRITERIA_1",
                Name = "Tiêu chí 1"
            };

            _context.Criterias.Add(criteria2);

            var criteria3 = new Criteria()
            {
                Id = Guid.NewGuid(),
                Code = "CRITERIA_2",
                Name = "Tiêu chí 2"
            };

            _context.Criterias.Add(criteria3);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

