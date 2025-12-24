using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Sale_Saas.Application.Common.Interfaces;
using Sale_Saas.Application.Features.FeaturePermissionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;

namespace Sale_Saas.Infrastructure.Services.ApplicationService
{
    public class FeaturePermissionServices : IFeaturePermissionService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly TenantDbContext _tenantContext;
        private readonly IApplicationRoleService _roleService;
        private readonly IMapper _mapper;

        public FeaturePermissionServices(
            IApplicationDbContext context,
            IMapper mapper, TenantDbContext tenantContext,
            IApplicationRoleService roleService,
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _tenantContext = tenantContext;
            _roleService = roleService;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<bool> ContainsPermission(string menu, List<string> features, Guid user, bool? isThrow = false)
        {
            var menuId = await _context.Menus.Where(s => s.Code == menu).Select(s => s.Id).FirstOrDefaultAsync();
            if (menuId == Guid.Empty)
            {
                if (isThrow == true) throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {menuId}");
                return false;
            }
            var roles = (await _roleService.GetListRoleByUserId(user)).Select(s => s.Id).ToList();
            var access = await _context.ApplicationRoleDetails
                                       .Where(s => s.ApplicationRoleId != null && roles.Contains((Guid)s.ApplicationRoleId) &&
                                                   s.FeatureId != null && features.Contains(s.FeatureId) && s.MenuId == menuId)
                                       .FirstOrDefaultAsync();
            if (access == null)
            {
                if (isThrow == true) throw new ApplicationException($"Tài khoản không có quyền truy cập");
                return false;
            }
            return true;
        }

        public async Task<bool> ContainsPosition(Guid UserId, string position)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(s => s.Id == UserId && s.DeleteFlag != true);
            if (user == null) return false;

            var names = (List<string>)await _userManager.GetRolesAsync(user);

            var data = await (from t1 in _context.ApplicationRoles
                              where t1.Name != null && names.Contains(t1.Name) && t1.RolePositionId == position
                              select t1
                             ).CountAsync();
            return data > 0;
        }

        public async Task<Dictionary<string, List<string>>> GetDictPermission(Guid Id)
        {
            var roles = (await _roleService.GetListRoleByUserId(Id)).Select(s => s.Id).ToList();

            var data = await _context.ApplicationRoleDetails
                .Where(s => s.MenuId != null && s.FeatureId != null && s.ApplicationRoleId != null && roles.Contains((Guid)s.ApplicationRoleId))
                .Include(s => s.Feature)
                .Include(s => s.Menu)
                .ToListAsync();

            var dictionary = data
                .GroupBy(s => new { s.Menu })
                .Select(g => new UserPermissionDto
                {
                    Menu = g.Key.Menu?.Code ?? "",
                    Features = g.Select(s => s.Feature!.Id).ToList()
                })
                .ToDictionary(
                    keySelector: dto => dto.Menu,
                    elementSelector: dto => dto.Features
                );

            return dictionary;
        }

        public async Task<List<string>> GetPolicy(Guid Id, string tenant)
        {
            var m_tenant = await _tenantContext.Tenants.FindAsync(tenant);
            if (m_tenant != null)
            {
                _context.SetConnectString(m_tenant.ConnectionString);
            }
            var roles = (await _roleService.GetListRoleByUserId(Id)).Select(s => s.Id).ToList();

            var data = await _context.ApplicationRoleDetails
                .Where(s => s.MenuId != null && s.FeatureId != null && s.ApplicationRoleId != null && roles.Contains((Guid)s.ApplicationRoleId) && s.DeleteFlag != true)
                .Include(s => s.Feature)
                .Include(s => s.Menu)
                .ToListAsync();

            var dtos = data
                .GroupBy(s => s.Menu?.Code)
                .SelectMany(g => g.Select(s => $"{g.Key}_{s.Feature!.Id}"))
                .ToList();

            return dtos;
        }

        public async Task<string> GetPositionByUser(Guid UserId)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(s => s.Id == UserId && s.DeleteFlag != true);
            if (user == null) return "";

            var names = (List<string>)await _userManager.GetRolesAsync(user);


            var data = await (from t1 in _context.ApplicationRoles
                              join t2 in _context.RolePositions on t1.RolePositionId equals t2.Id
                              where t1.Name != null && names.Contains(t1.Name)
                              orderby t2.Level ascending
                              select t1.RolePositionId).FirstOrDefaultAsync();
            return data ?? "";
        }

        public async Task<bool> HasPermission(string menu, string feature, Guid user, bool? isThrow = false)
        {
            var roles = (await _roleService.GetListRoleByUserId(user)).Select(s => s.Id).ToList();
            var access = await _context.ApplicationRoleDetails
                                       .Include(s => s.Menu)
                                       .Where(s => s.ApplicationRoleId != null && roles.Contains((Guid)s.ApplicationRoleId) &&
                                                   s.FeatureId == feature && s.Menu != null && s.Menu.Code == menu)
                                       .FirstOrDefaultAsync();
            if (access == null)
            {
                if (isThrow == true) throw new ApplicationException($"Tài khoản không có quyền truy cập");
                return false;
            }
            return true;
        }
    }
}
