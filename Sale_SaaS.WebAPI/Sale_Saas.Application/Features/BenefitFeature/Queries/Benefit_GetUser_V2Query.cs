using Sale_Saas.Application.Features.BenefitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.BenefitFeature.Queries
{
    public record Benefit_GetUser_V2Query(Guid userId, GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<UserBasicInfoDto>>>;

    public class Benefit_GetUser_V2QueryHandler : IRequestHandler<Benefit_GetUser_V2Query, Result<IEnumerable<UserBasicInfoDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IFeaturePermissionService _permissionService;
        private readonly IApplicationRoleService _applicationRoleService;
        private readonly IApplicationUserService _userService;
        private readonly IEventLogService _eventLogService;

        public Benefit_GetUser_V2QueryHandler(
               IMapper mapper,
               IApplicationDbContext context,
               IFeaturePermissionService permissionService,
               IApplicationRoleService applicationRoleService,
               IApplicationUserService userService,
               IEventLogService eventLogService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _applicationRoleService = applicationRoleService;
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _eventLogService = eventLogService ?? throw new ArgumentNullException(nameof(eventLogService));
        }

        public async Task<Result<IEnumerable<UserBasicInfoDto>>> Handle(Benefit_GetUser_V2Query request, CancellationToken cancellationToken)
        {
            if (request.RequestData.UserId == null)
            {
                throw new ApplicationException("Missing user ID in request data");
            }

            var userId = (Guid)request.RequestData.UserId;

            var position = await _permissionService.GetPositionByUser(userId);

            var distinctIds = await GetDistinctBenefitUserIds();

            //var ids = await GetFilteredUserIds(position, distinctIds);
            var userRoles= await GetFilteredUserRoleIds(position, distinctIds);
            var ids= userRoles.Select(x=>x.UserId).ToList();
            var query = _context.ApplicationUsers
                                     .Where(con => con.DeleteFlag != true && ids.Contains(con.Id) && !con.Id.Equals(userId))
                                     .Select(con => _mapper.Map<UserBasicInfoDto>(con))
                                     .AsNoTracking();

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(s => s.FullName.Contains(request.RequestData.TextSearch) ||
                                               s.Code.Contains(request.RequestData.TextSearch) ||
                                               s.Email.Contains(request.RequestData.TextSearch));
            }
            var users = await query.ToListAsync();

            var eventLog = await _eventLogService.Create("BenefitFeature", "BenefitFeature", "Benefit_GetUser_V2Query", request.userId);

            return Result<IEnumerable<UserBasicInfoDto>>.Success(users);
        }

        private async Task<List<UserRoleDto>> GetFilteredUserRoleIds(string position, List<BenefitRoleCountDto> distincts)
        {
            var userRoles = new List<UserRoleDto>();

            if (position == RolePositionEnum.MANAGER.ToString())
            {
                distincts = distincts.Where(x => x.RolePositionId == RolePositionEnum.EMPLOYEE.ToString()).ToList();
                userRoles = await _userService.GetUserRoleByRolePositionAndPermission(
                     RolePositionEnum.EMPLOYEE.ToString(),
                     MenuType.Sale_QL,
                     FeatureType.QL_THUAHUONG
                );

                for (var i = distincts.Count - 1; i >= 0; i--)
                {
                    if (distincts[i].RolePositionId == RolePositionEnum.EMPLOYEE.ToString())
                    {
                        var countUser = (await _applicationRoleService.GetListRoleStringByUserIdAndRolePositionId(distincts[i].ApplicationUserId ?? Guid.Empty, RolePositionEnum.EMPLOYEE.ToString(), ""))
                                        .Count();
                        if (countUser > distincts[i].QuantityOfRole)
                            distincts.RemoveAt(i);
                    }
                }
                userRoles = userRoles.Where(s => !distincts.Any(p => p.ApplicationUserId == s.UserId && p.ApplicationRoleId == s.RoleId)).ToList();
                return userRoles;
            }
            else if (position == RolePositionEnum.ADMINISTRATOR.ToString())
            {
                distincts = distincts.Where(x => x.RolePositionId == RolePositionEnum.MANAGER.ToString()).ToList();
                userRoles = await _userService.GetUserRoleByRolePositionAndPermission(
                     RolePositionEnum.MANAGER.ToString(),
                     MenuType.Sale_QL,
                     FeatureType.QL_THUAHUONG
                );

                distincts = distincts.Where(x => x.RolePositionId == RolePositionEnum.MANAGER.ToString()).ToList();

                userRoles = userRoles.Where(s => !distincts.Any(p => p.ApplicationUserId == s.UserId && p.ApplicationRoleId == s.RoleId)).ToList();
                return userRoles;
            }
            else
            {
                return new List<UserRoleDto>();
            }
        }

        private async Task<List<Guid>> GetFilteredUserIds(string position, List<BenefitRoleCountDto> distincts)
        {
            var ids = new List<Guid>();

            if (position == RolePositionEnum.MANAGER.ToString())
            {
                distincts = distincts.Where(x => x.RolePositionId == RolePositionEnum.EMPLOYEE.ToString()).ToList();
                ids = await _userService.GetUserIdByRolePositionAndPermission(
                     RolePositionEnum.EMPLOYEE.ToString(),
                     MenuType.Sale_QL,
                     FeatureType.QL_THUAHUONG
                );

                for (var i = distincts.Count - 1; i >= 0; i--)
                {
                    if (distincts[i].RolePositionId == RolePositionEnum.EMPLOYEE.ToString())
                    {
                        var countUser = (await _applicationRoleService.GetListRoleStringByUserIdAndRolePositionId(distincts[i].ApplicationUserId ?? Guid.Empty, RolePositionEnum.EMPLOYEE.ToString(), ""))
                                        .Count();
                        if (countUser > distincts[i].QuantityOfRole)
                            distincts.RemoveAt(i);
                    }
                }

                List<Guid> distinctIds = distincts.Select(x => x.ApplicationUserId ?? Guid.Empty).Distinct().ToList();
                ids = ids.Where(s => !distinctIds.Contains(s)).ToList();
                return ids;
            }
            else if (position == RolePositionEnum.ADMINISTRATOR.ToString())
            {
                distincts = distincts.Where(x => x.RolePositionId == RolePositionEnum.MANAGER.ToString()).ToList();
                var userRoles = await _userService.GetUserRoleByRolePositionAndPermission(
                     RolePositionEnum.MANAGER.ToString(),
                     MenuType.Sale_QL,
                     FeatureType.QL_THUAHUONG
                );

                distincts = distincts.Where(x => x.RolePositionId == RolePositionEnum.MANAGER.ToString()).ToList();

                List<Guid> distinctIds = distincts.Select(x => x.ApplicationUserId ?? Guid.Empty).Distinct().ToList();
                userRoles = userRoles.Where(s => !distincts.Any(p=> p.ApplicationUserId==s.UserId && p.ApplicationRoleId==s.RoleId)).ToList();
                return ids;
            }
            else
            {
                return new List<Guid>();
            }
        }

        private async Task<List<BenefitRoleCountDto>> GetDistinctBenefitUserIds()
        {
            var currentYear = DateTime.Now.Year;

            var idsNullable = await _context.Benefits.Where(x => x.DeleteFlag != true && x.ApplicationUserId != null && x.CreatedDate.Year == currentYear)
                                                     .GroupBy(x => new { x.ApplicationUserId, x.RolePositionId, x.ApplicationRoleId })
                                                     .Select(x => new BenefitRoleCountDto()
                                                     {
                                                         ApplicationUserId = x.Key.ApplicationUserId,
                                                         RolePositionId = x.Key.RolePositionId,
                                                         ApplicationRoleId=x.Key.ApplicationRoleId,
                                                         QuantityOfRole = x.Count(),
                                                        
                                                     })
                                                     .ToListAsync();

            return idsNullable;
        }
    }
}
