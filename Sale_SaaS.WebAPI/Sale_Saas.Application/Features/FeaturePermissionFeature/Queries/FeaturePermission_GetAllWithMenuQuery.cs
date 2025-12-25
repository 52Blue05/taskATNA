using Sale_Saas.Application.Features.FeaturePermissionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Domain.Enums;
using System.Data;

namespace Sale_Saas.Application.Features.FeaturePermissionFeature.Queries;

public record FeaturePermission_GetAllWithMenuQuery(Guid userId, GetAllQueryRequest RequestData) : IRequest<Result<List<MenuWithFeatureDto>>>;
public class FeaturePermission_GetAllWithMenuQueryHandler : IRequestHandler<FeaturePermission_GetAllWithMenuQuery, Result<List<MenuWithFeatureDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IEventLogService _eventLogService;

    public FeaturePermission_GetAllWithMenuQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _mapper = mapper;
        _eventLogService = eventLogService;
    }

    public async Task<Result<List<MenuWithFeatureDto>>> Handle(FeaturePermission_GetAllWithMenuQuery request, CancellationToken cancellationToken)
    {
        if (request.RequestData.RoleId == null || request.RequestData.RolePositionId == null)
        {
            throw new ApplicationException("Không có dữ liệu gửi đến máy chủ");
        }

        var role = await _context.ApplicationRoles.FindAsync(request.RequestData.RoleId);
        if (role == null)
        {
            throw new ApplicationException("Không có dữ liệu gửi đến máy chủ");
        }
        var access = await _context.RolePositionFeatureMenus.Where(s => s.RolePositionId == request.RequestData.RolePositionId).Select(s => s.FeatureMenuId).ToListAsync();

        var permission = await _context.ApplicationRoleDetails
                                       .Where(s => s.ApplicationRoleId == role.Id && s.DeleteFlag != true && s.FeatureId != null && s.MenuId != null)
                                       .ToListAsync();

        var featureMenus = await (from fm in _context.FeatureMenus
                                  join menu in _context.Menus on fm.MenuId equals menu.Id
                                  join feature in _context.Features on fm.FeatureId equals feature.Id
                                  where menu.DeleteFlag != true && feature.DeleteFlag != true && access.Contains(fm.Id) && menu.IsActivite == true
                                  select new
                                  {
                                      FMId = fm.Id,
                                      Menu = menu,
                                      Feature = feature
                                  }).AsNoTracking().ToListAsync();

        var groupedMenus = featureMenus.GroupBy(fm => fm.Menu.Id)
                                       .Select(g => new MenuWithFeatureDto
                                       {
                                           Id = g.Key,
                                           Name = g.First().Menu.Name,
                                           Code = g.First().Menu.Code,
                                           ParentId = g.First().Menu.ParentId,
                                           SortOrder = g.First().Menu.SortOrder,
                                           Features = g.Select(fm => new FeaturePermissionDto
                                           {
                                               Id = fm.Feature.Id,
                                               Name = fm.Feature.Name,
                                               Access = false,
                                               Sort = fm.Feature.Sort,
                                               FMId = fm.FMId,

                                           }).OrderBy(s => s.Sort).ToList()
                                       })
                                       .ToList();

        var Sale_Roles = groupedMenus.Where(r => r.Code != null && r.Code.StartsWith("Sale_")).OrderBy(r => r.SortOrder);
        var NS_Roles = groupedMenus.Where(r => r.Code != null && r.Code.StartsWith("NS_")).OrderBy(r => r.SortOrder);
        var DM_Roles = groupedMenus.Where(r => r.Code != null && r.Code.StartsWith("DM_")).OrderBy(r => r.SortOrder);

        var sortedRoles = Sale_Roles.Concat(NS_Roles).Concat(DM_Roles).ToList();
        List<FeaturePermissionDto> listItem = null;
        foreach (var menu in sortedRoles)
        {
            listItem = new List<FeaturePermissionDto>();

            foreach (var feature in menu.Features)
            {
                var exist = permission.FirstOrDefault(s => s.MenuId == menu.Id && s.FeatureId == feature.Id);

                if (request.RequestData.RolePositionId == RolePositionEnum.EMPLOYEE.ToString()
                    && menu.Code.StartsWith(MenuType.Sale_SK.ToString())
                    && feature.Id == FeatureType.SALEKIT_XEMPHANQUYEN.ToString())
                {
                    continue;
                }
                if (request.RequestData.RolePositionId == RolePositionEnum.EMPLOYEE.ToString()
                    && menu.Code.StartsWith(MenuType.NS_TTTN.ToString())
                    && feature.Id == FeatureType.IMPORT_EXCEL.ToString())
                {
                    continue;
                }

                if (menu.Code.StartsWith(MenuType.Sale_MT.ToString()) &&
                        (feature.Id == FeatureType.IMPORT_EXCEL.ToString()
                            || feature.Id == FeatureType.EXPORT_EXCEL.ToString()
                            || feature.Id == FeatureType.DETAIL.ToString()))
                {
                    continue;
                }

                // start new-update

                if (menu.Code.StartsWith(MenuType.Sale_CH.ToString())
                            && feature.Id == FeatureType.CH_XEMCAPNHAT.ToString())
                {
                    feature.Name = "Lịch sử tương tác";
                }

                // end new-update

                if (((menu.Code.StartsWith("DM_") || menu.Code.StartsWith(MenuType.NS_TTNS.ToString()))
                    && (feature.Id == FeatureType.IMPORT_EXCEL.ToString()
                    || feature.Id == FeatureType.EXPORT_EXCEL.ToString() || feature.Id == FeatureType.MYSELF.ToString()))
                    || (menu.Code.StartsWith(MenuType.NS_TTTN.ToString()) &&
                        (feature.Id == FeatureType.EXPORT_EXCEL.ToString())))
                {
                    continue;
                }
                if (request.RequestData.RolePositionId == RolePositionEnum.ADMINISTRATOR.ToString()
                    && menu.Code.StartsWith(MenuType.Sale_MT.ToString())
                    && feature.Id == FeatureType.UPDATE_RESULT.ToString())
                {
                    feature.Name = "Cập nhật kết quả";
                }
                if (request.RequestData.RolePositionId == RolePositionEnum.ADMINISTRATOR.ToString()
                    && menu.Code.StartsWith(MenuType.Sale_MQH.ToString())
                    && feature.Id == FeatureType.UPDATE_RESULT.ToString())
                {
                    feature.Name = "Cập nhật điểm thực tế";
                }

                if (menu.Code.StartsWith(MenuType.Sale_EL_GD.ToString()) && feature.Id == FeatureType.EL_GD_THEMKHOA.ToString())
                {
                    feature.Name = "Thêm và chỉnh sửa khoá học vào chương trình";
                }
                if (menu.Code.StartsWith(MenuType.Sale_MQH.ToString()))
                {
                    if (feature.Id == FeatureType.MQH_CAPNHATBANGGAINS.ToString())
                    {
                        feature.Name = "Cập nhật kết quả";
                    }

                    if (feature.Id == FeatureType.MQH_DANHGIA.ToString())
                    {
                        feature.Name = "Đánh giá mối quan hệ";
                    }
                }
                listItem.Add(feature);
                feature.Access = false;
                if (exist != null) feature.Access = true;
                menu.Features = listItem;
            }
        }
        var eventLog = await _eventLogService.Create("FeaturePermissionFeature", "FeaturePermissionFeature",
                                                "FeaturePermission_GetAllWithMenuQuery", request.userId);

        return Result<List<MenuWithFeatureDto>>.Success(sortedRoles);
    }

    private void AddMenuWithChildren(List<MenuWithFeatureDto> sortedList, MenuWithFeatureDto menu, Dictionary<Guid, MenuWithFeatureDto> menuDictionary)
    {
        sortedList.Add(menu);

        var childMenus = menuDictionary.Values.Where(m => m.ParentId == menu.Id).OrderBy(m => m.SortOrder).ToList();

        foreach (var childMenu in childMenus)
        {
            AddMenuWithChildren(sortedList, childMenu, menuDictionary);
        }
    }

}
