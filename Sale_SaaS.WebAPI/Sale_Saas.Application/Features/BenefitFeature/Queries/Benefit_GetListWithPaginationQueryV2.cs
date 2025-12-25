using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.BenefitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.BenefitFeature.Queries
{
    public record Benefit_GetListWithPaginationQueryV2(Guid userId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<BenefitDto>>>;

    public class Benefit_GetListWithPaginationQueryV2Handler : IRequestHandler<Benefit_GetListWithPaginationQueryV2, Result<PaginatedList<BenefitDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IApplicationUserService _userService;
        private readonly IFeaturePermissionService _permissionService;
        private readonly IApplicationRoleService _roleService;
        private readonly IEventLogService _eventLogService;

        public Benefit_GetListWithPaginationQueryV2Handler(
            IMapper mapper, IApplicationDbContext context, IApplicationUserService userService, IFeaturePermissionService permissionService,
            IApplicationRoleService roleService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _permissionService = permissionService;
            _roleService = roleService;
            _eventLogService = eventLogService;
        }

        public async Task<Result<PaginatedList<BenefitDto>>> Handle(Benefit_GetListWithPaginationQueryV2 request, CancellationToken cancellationToken)
        {
            var query = _context.Benefits.Where(m => m.DeleteFlag != true && m.ApplicationUser != null && m.ApplicationUser.DeleteFlag != true)
                                         .Include(s => s.ApplicationUser)
                                         .Include(s => s.BenefitStatus)
                                         .Include(s => s.ApplicationRole)
                                         .OrderByDescending(x => x.CreatedDate)
                                         .ProjectTo<BenefitDto>(_mapper.ConfigurationProvider)
                                         .AsNoTracking();
            var temp = query.ToList();
            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(s => s.ApplicationUser!.LastName.ToLower().Contains(request.RequestData.TextSearch.ToLower()) ||
                                         s.ApplicationUser!.FirstName.ToLower().Contains(request.RequestData.TextSearch.ToLower()) ||
                                         s.ApplicationUser!.FullName.ToLower().Contains(request.RequestData.TextSearch.ToLower()));
            }
            if (request.RequestData.RoleType != null)
            {
                var user = request.userId;
                if (request.RequestData.RoleType == RoleType.MYSELF.ToString())
                {
                    if (request.RequestData.UserId == null)
                    {
                        throw new ApplicationException("Không tìm thấy người dùng");
                    }
                    await _permissionService.HasPermission(
                              MenuType.Sale_QL,
                              FeatureType.MYSELF,
                              user,
                              true
                         );

                    var listRoleOfUser = (await _roleService.GetListRoleByUserId(request.userId)).Select(x => x.Id).ToList();

                    query = query.Where(s => s.ApplicationUser.Id == request.RequestData.UserId && s.ApplicationRoleId.HasValue && listRoleOfUser.Contains(s.ApplicationRoleId.Value));
                }
                else if (request.RequestData.RoleType == RoleType.MANAGER.ToString())
                {
                    await _permissionService.HasPermission(
                         MenuType.Sale_QL,
                         FeatureType.MANAGER,
                         user,
                         true
                    );

                    var ids = await _userService.GetUserIdByRolePosition(RolePositionEnum.MANAGER.ToString());
                    query = query.Where(s => ids.Contains(s.ApplicationUser.Id));
                    if (request.RequestData.UserId != null)
                    {
                        query = query.Where(s => s.ApplicationUser.Id == request.RequestData.UserId);
                    }

                    query = query.Where(s => s.RolePositionId == request.RequestData.RoleType);
                    query = query.Where(s => s.ApplicationUser.Id != request.userId);
                }
                else if (request.RequestData.RoleType == RoleType.EMPLOYEE.ToString())
                {
                    await _permissionService.HasPermission(
                         MenuType.Sale_QL,
                         FeatureType.EMPLOYEE,
                         user,
                         true
                    );

                    var ids = await _userService.GetUserIdByRolePosition(RolePositionEnum.EMPLOYEE.ToString());
                    query = query.Where(s => ids.Contains(s.ApplicationUser.Id));
                    if (request.RequestData.UserId != null)
                    {
                        query = query.Where(s => s.ApplicationUser.Id == request.RequestData.UserId);
                    }

                    query = query.Where(s => s.RolePositionId == request.RequestData.RoleType);
                    query = query.Where(s => s.ApplicationUser.Id != request.userId);
                }
                else
                {
                    throw new Exception($"Role type không hợp lệ: {request.RequestData.RoleType}");
                }
            }

            if (request.RequestData.StatusId != null)
            {
                query = query.Where(s => s.BenefitStatus.Id == request.RequestData.StatusId);
            }

            if (request.RequestData.Time != null)
            {
                query = query.Where(s => s.CreatedDate.Year == request.RequestData.Time.Value.Year);
            }

            if (request.RequestData.RoleId != null)
            {
                query = query.Where(s => s.ApplicationRoleId == request.RequestData.RoleId);
            }

            var eventLog = await _eventLogService.Create("BenefitFeature", "BenefitFeature", "Benefit_GetListWithPaginationQueryV2", request.userId);

            return Result<PaginatedList<BenefitDto>>.Success(await query.PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize));
        }
    }
}
