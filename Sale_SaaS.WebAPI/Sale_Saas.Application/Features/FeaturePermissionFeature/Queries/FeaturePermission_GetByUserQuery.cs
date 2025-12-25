using Sale_Saas.Application.Features.FeaturePermissionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.FeaturePermissionFeature.Queries
{
    public record FeaturePermission_GetByUserQuery(Guid userId) : IRequest<Result<Dictionary<string, List<string>>>>;
    public class FeaturePermission_GetByUserQueryHandler : IRequestHandler<FeaturePermission_GetByUserQuery, Result<Dictionary<string, List<string>>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IApplicationRoleService _roleService;
        private readonly IEventLogService _eventLogService;

        public FeaturePermission_GetByUserQueryHandler(IMapper mapper, IApplicationDbContext context,
                                                            IApplicationRoleService roleService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _roleService = roleService;
            _eventLogService = eventLogService;
        }

        public async Task<Result<Dictionary<string, List<string>>>> Handle(FeaturePermission_GetByUserQuery request, CancellationToken cancellationToken)
        {
            var roles = (await _roleService.GetListRoleByUserId(request.userId)).Select(s => s.Id).ToList();

            var data = await _context.ApplicationRoleDetails
                .Where(s => s.MenuId != null && s.FeatureId != null && s.ApplicationRoleId != null && roles.Contains((Guid)s.ApplicationRoleId))
                .Include(s => s.Feature)
                .Include(s => s.Menu)
                .ToListAsync();

            var dictionary = data
                .GroupBy(s => new { Menu = s.Menu })
                .Select(g => new UserPermissionDto
                {
                    Menu = g.Key.Menu?.Code ?? "",
                    Features = g.Select(s => s.Feature!.Id).Distinct().ToList()
                })
                .ToDictionary(
                    keySelector: dto => dto.Menu,
                    elementSelector: dto => dto.Features
                );

            var managerRole = await _context.ApplicationRoles.Where(ar => ar.RolePositionId == RolePositionEnum.MANAGER.ToString()).Select(ar => ar.Id).FirstOrDefaultAsync();
            var administratorRole = await _context.ApplicationRoles.Where(ar => ar.RolePositionId == RolePositionEnum.ADMINISTRATOR.ToString()).Select(ar => ar.Id).FirstOrDefaultAsync();

            if (dictionary.ContainsKey(MenuType.Sale_MT.ToString()) && (roles.Contains(managerRole) || roles.Contains(administratorRole)))
            {
                var saleMTElements = dictionary[MenuType.Sale_MT.ToString()];

                if (!saleMTElements.Contains(FeatureType.MYSELF.ToString()) && !saleMTElements.Contains(FeatureType.EMPLOYEE.ToString()) && !saleMTElements.Contains(FeatureType.MANAGER.ToString()))
                {
                    dictionary.Remove(MenuType.Sale_MT.ToString());
                }
            }

            var eventLog = await _eventLogService.Create("FeaturePermissionFeature", "FeaturePermissionFeature",
                                                "FeaturePermission_GetByUserQuery", request.userId);

            return Result<Dictionary<string, List<string>>>.Success(dictionary);
        }
    }
}
