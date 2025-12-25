using Sale_Saas.Application.Features.FeaturePermissionFeature.Dto;
using Sale_Saas.Application.Features.MenuFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.MenuFeature.Queries;

public record Menu_GetByUserQuery(Guid Id, string isAdminAta) : IRequest<Result<List<CategoryMenuDto>>>;
public class Menu_GetByUserQueryHandler : IRequestHandler<Menu_GetByUserQuery, Result<List<CategoryMenuDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IApplicationRoleService _roleService;
    private readonly IApplicationUserService _userService;
    private readonly IMapper _mapper;
    public Menu_GetByUserQueryHandler(
         IMapper mapper, IApplicationDbContext context, IApplicationRoleService roleService, IApplicationUserService userService)
    {
        _context = context;
        _mapper = mapper;
        _roleService = roleService;
        _userService = userService;
    }

    public async Task<Result<List<CategoryMenuDto>>> Handle(Menu_GetByUserQuery request, CancellationToken cancellationToken)
    {
        List<CategoryMenuDto> datas = new List<CategoryMenuDto>();

        if (!request.isAdminAta.Equals("NoAdminAta"))
        {
            CategoryMenuDto management = new CategoryMenuDto()
            {
                Id = Guid.Empty,
                Code = "dashboard",
                Icon = "dashboard",
                Path = "/saas/dashboard",
                Label = new CategoryLabel()
                {
                    en_US = "Dashboard",
                    vi_VN = "Dashboard",
                }
            };

            datas.Add(management);

            CategoryMenuDto servicePackageManagement = new CategoryMenuDto()
            {
                Id = Guid.Empty,
                Code = "service-package-management",
                Icon = "service-management",
                Path = "/saas/service-package-management",
                Label = new CategoryLabel()
                {
                    en_US = "Service Package Management",
                    vi_VN = "Quản lý gói dịch vụ",
                }
            };

            datas.Add(servicePackageManagement);

            CategoryMenuDto companyManagement = new CategoryMenuDto()
            {
                Id = Guid.Empty,
                Code = "company-management",
                Icon = "company-management",
                Path = "/saas/company-management",
                Label = new CategoryLabel()
                {
                    en_US = "Company Management",
                    vi_VN = "Quản lý công ty",
                }
            };
            datas.Add(companyManagement);
        }
        else
        {
            bool isAdmin = await _userService.CheckIsAdmin(request.Id);
            if (isAdmin == true)
            {
                CategoryMenuDto data = new CategoryMenuDto()
                {
                    Id = request.Id,
                    Code = "management",
                    Icon = "management",
                    Path = "",
                    Label = new CategoryLabel()
                    {
                        en_US = "Management",
                        vi_VN = "Quản lý",
                    },
                    Children = new List<CategoryChildrenMenuDto>()
                {
                    new CategoryChildrenMenuDto() {
                        Id = request.Id,
                        Code = "account",
                        Icon = "dot",
                        Path = "/management/account" ,
                        Label = new CategoryLabel()
                        {
                            en_US = "Account",
                            vi_VN = "Tài khoản",
                        }
                    },
                    new CategoryChildrenMenuDto() {
                        Id = request.Id,
                        Code = "organize",
                        Icon = "dot",
                        Path = "/management/organize" ,
                        Label = new CategoryLabel()
                        {
                            en_US = "Organize",
                            vi_VN = "Tổ chức",
                        }
                    },
                    new CategoryChildrenMenuDto() {
                        Id = request.Id,
                        Code = "position",
                        Icon = "dot",
                        Path = "/management/position" ,
                        Label = new CategoryLabel()
                        {
                            en_US = "Position",
                            vi_VN = "Chức vụ",
                        }
                    },
                }
                };
                datas.Add(data);
            }

            // Check Role Current
            List<ApplicationRole> roles = await _roleService.GetListRoleByUserId(request.Id);
            if (!roles.Any()) return Result<List<CategoryMenuDto>>.Success(datas);
            if (roles.Count == 1 && isAdmin == true)
            {
                return Result<List<CategoryMenuDto>>.Success(datas);
            }
            var ids = roles.Select(s => s.Id).ToList();

            var dataRoleDetail = await _context.ApplicationRoleDetails
                                     .Where(s => s.MenuId != null && s.FeatureId != null && s.ApplicationRoleId != null && ids.Contains((Guid)s.ApplicationRoleId))
                                     .Include(s => s.Feature)
                                     .Include(s => s.Menu)
                                     .ToListAsync();

            List<Guid?> menuIds = dataRoleDetail.Where(s => s.FeatureId != FeatureType.SEARCH).Select(s => s.MenuId).ToList();

            var menus = await _context.Menus
                                        .Where(s => s.ParentId == null
                                                || (s.ParentId != null && menuIds.Contains(s.Id)))
                                        .ToListAsync();
            if (menus.Any())
            {
                var dataByRoles = menus.Where(s => s.ParentId == null).OrderBy(s => s.SortOrder)
                             .Select(s => new CategoryMenuDto
                             {
                                 Id = s.Id,
                                 Code = s.NameAction ?? string.Empty,
                                 Path = s.BreadcrumbNavigation ?? string.Empty,
                                 Icon = s.Icon ?? string.Empty,
                                 Label = new CategoryLabel()
                                 {
                                     vi_VN = s.Name ?? "",
                                     en_US = s.NameEn ?? "",
                                 },
                                 Children = new List<CategoryChildrenMenuDto>()
                             }).ToList();

                for (int i = dataByRoles.Count - 1; i >= 0; i--)
                {
                    var data = dataByRoles[i];
                    data.Children = menus.Where(s => s.ParentId == data.Id).OrderBy(s => s.SortOrder)
                                        .Select(s => new CategoryChildrenMenuDto
                                        {
                                            Id = s.Id,
                                            Code = s.NameAction ?? string.Empty,
                                            Path = s.BreadcrumbNavigation ?? string.Empty,
                                            Icon = s.Icon ?? string.Empty,
                                            Label = new CategoryLabel()
                                            {
                                                vi_VN = s.Name ?? "",
                                                en_US = s.NameEn ?? "",
                                            }
                                        }).ToList();

                    if (data.Children == null || !data.Children.Any())
                    {
                        dataByRoles.RemoveAt(i);
                    }
                }
                datas.AddRange(dataByRoles);
            }

            var dictionary = dataRoleDetail
                 .GroupBy(s => new { Menu = s.Menu })
                 .Select(g => new UserPermissionDto
                 {
                     Menu = g.Key.Menu?.Code ?? "",
                     Features = g.Select(s => s.Feature!.Id).ToList()
                 })
                 .ToDictionary(
                      keySelector: dto => dto.Menu,
                      elementSelector: dto => dto.Features
                 );

            var managerRole = await _context.ApplicationRoles.Where(ar => ar.RolePositionId == RolePositionEnum.MANAGER.ToString()).Select(ar => ar.Id).FirstOrDefaultAsync();
            var administratorRole = await _context.ApplicationRoles.Where(ar => ar.RolePositionId == RolePositionEnum.ADMINISTRATOR.ToString()).Select(ar => ar.Id).FirstOrDefaultAsync();

            List<string> dictionaryInitKeys = dictionary.Keys.ToList();

            foreach (var key in dictionaryInitKeys)
            {
                if (key == MenuType.NS_TTTN.ToString())
                {
                    var saleMTElements = dictionary[key];

                    if (!saleMTElements.Contains(FeatureType.TTTN_HIENTAI_MYSELF.ToString()) && !saleMTElements.Contains(FeatureType.TTTN_HIENTAI_ALL.ToString()) &&
                         !saleMTElements.Contains(FeatureType.TTTN_VITRI_MYSELF.ToString()) && !saleMTElements.Contains(FeatureType.TTTN_VITRI_ALL.ToString()))
                    {
                        dictionary.Remove(key);
                    }
                    continue;
                }

                if (dictionary.ContainsKey(key) && (ids.Contains(managerRole) || ids.Contains(administratorRole)))
                {
                    var saleMTElements = dictionary[key];

                    if (!saleMTElements.Contains(FeatureType.VIEW.ToString()))
                    {
                        //if (!saleMTElements.Contains(FeatureType.MYSELF.ToString()) && !saleMTElements.Contains(FeatureType.EMPLOYEE.ToString()))
                        //{
                        //    dictionary.Remove(key);
                        //}

                        if (ids.Contains(administratorRole) && !saleMTElements.Contains(FeatureType.MYSELF.ToString()) && !saleMTElements.Contains(FeatureType.EMPLOYEE.ToString()) && !saleMTElements.Contains(FeatureType.MANAGER.ToString()))
                        {
                            dictionary.Remove(key);
                        }
                        else if (ids.Contains(managerRole) && !saleMTElements.Contains(FeatureType.MYSELF.ToString()) && !saleMTElements.Contains(FeatureType.EMPLOYEE.ToString()))
                        {
                            dictionary.Remove(key);
                        }
                    }
                }
                else
                {
                    var saleMTElements = dictionary[key];

                    if (!saleMTElements.Contains(FeatureType.VIEW.ToString()))
                    {
                        if ((!saleMTElements.Contains(FeatureType.MYSELF.ToString())))
                        {
                            dictionary.Remove(key);
                        }
                    }
                }
            }

            var dictionaryKeys = dictionary.Keys.ToList();
            var menuDictionary = MenuDetailMapper.MenuDetailToString;

            List<string> getMenuEnglish = new List<string>();

            foreach (var items in dictionaryKeys)
            {
                getMenuEnglish.Add(menuDictionary[items]);
            }

            for (int i = datas.Count - 1; i >= 0; i--)
            {
                for (int j = datas[i].Children.Count - 1; j >= 0; j--)
                {
                    var checkMenu = getMenuEnglish.Where(s => s.Equals(datas[i].Children[j].Code));

                    if (!checkMenu.Any())
                        datas[i].Children.RemoveAt(j);
                }

                if (datas[i].Children.Count <= 0)
                {
                    datas.RemoveAt(i);
                    continue;
                }
            }
        }

        return Result<List<CategoryMenuDto>>.Success(datas);
    }
}
