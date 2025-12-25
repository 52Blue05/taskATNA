using Sale_Saas.Application.Features.SaleKitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Role_SaleKit;
using Sale_Saas.Domain.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sale_Saas.Application.Features.SaleKitFeature.Queries;

public record SaleKitMobile_GetListWithPaginationByRoleQuery(Guid UserId, RoleSaleKitGetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<SaleKitDto>>>;

public class SaleKitMobile_GetListWithPaginationByRoleQueryHandler : IRequestHandler<SaleKitMobile_GetListWithPaginationByRoleQuery, Result<PaginatedList<SaleKitDto>>>
{
    private readonly IApplicationRoleService _roleService;
    private readonly IFeaturePermissionService _permissionService;
    private readonly IApplicationDbContext _context;
    private readonly IApplicationUserService _userService;
    private readonly IMapper _mapper;

    public SaleKitMobile_GetListWithPaginationByRoleQueryHandler(IMapper mapper, IApplicationDbContext context, IApplicationRoleService roleService, IFeaturePermissionService permissionService, IApplicationUserService userService)
    {
        _context = context;
        _mapper = mapper;
        _roleService = roleService;
        _permissionService = permissionService;
        _userService = userService;
    }

    public async Task<Result<PaginatedList<SaleKitDto>>> Handle(SaleKitMobile_GetListWithPaginationByRoleQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy dữ liệu");
        }

        // get list role of user
        var roles = await _userService.GetApplicationRolesByUserIdAsync(request.UserId);

        IQueryable<SaleKitDto> salekits;

        // check user has SALEKIT_XEMPHANQUYEN or not ?
        var rolePositionIds = roles.Select(x => x.RolePositionId).ToList();
        var roleIds = roles.Select(x => x.Id).ToList();

        if (roleIds == null || roleIds.Count <= 0)
        {
            throw new ApplicationException("Không lấy được quyền của user này");
        }

        var applicationRoleDetails = await _context.ApplicationRoleDetails.Where(x => x.FeatureId == FeatureType.SALEKIT_XEMPHANQUYEN
                                                                                   && x.ApplicationRoleId.HasValue
                                                                                   && roleIds.Contains(x.ApplicationRoleId.Value))
                                                                          .ToListAsync();
        if (applicationRoleDetails != null && applicationRoleDetails.Count > 0)
        {
            // get list salekit
            salekits = _context.SaleKits.Where(x => x.DeleteFlag != true)
                                                .OrderByDescending(x => x.CreatedDate)
                                                .ProjectTo<SaleKitDto>(_mapper.ConfigurationProvider)
                                                .AsNoTracking();

            salekits = HandlerFilter(request, salekits);

            var response = salekits.ToList();

            foreach (var item in response)
            {
                item.CreatedUser = await _userService.GetUserBasicInforById(item.LastModifiedApplicationUserId ?? Guid.Empty);
            }

            return Result<PaginatedList<SaleKitDto>>.Success(new PaginatedList<SaleKitDto>(response, response.Count, request.RequestData.PageIndex, request.RequestData.PageSize));
        }

        // not have SALE_XEMPHANQUYEN => Filter by access = true
        var queryAppSaleKit = _context.ApplicationRole_SaleKits
                    .Where(x => (x.ApplicationRoleId.HasValue && roleIds.Contains(x.ApplicationRoleId.Value))
                        && x.Access == true && x.DeleteFlag != true);
        if(request.RequestData.RoleId!=null)
        {
            queryAppSaleKit = queryAppSaleKit.Where(s => s.ApplicationRoleId == request.RequestData.RoleId);
        }
        var salekitIds = await queryAppSaleKit
                    .Select(x => x.SaleKitId)
                    .Distinct()
                    .ToListAsync();
        salekits = _context.SaleKits.Where(m => m.DeleteFlag != true && salekitIds.Contains(m.Id)).
            OrderByDescending(x => x.CreatedDate)
                                    .ProjectTo<SaleKitDto>(_mapper.ConfigurationProvider)
                                    .AsNoTracking();
        salekits = HandlerFilter(request, salekits);

        var result = salekits.ToList();

        foreach (var item in result)
        {
            item.CreatedUser = await _userService.GetUserBasicInforById(item.LastModifiedApplicationUserId ?? Guid.Empty);
        }

        return Result<PaginatedList<SaleKitDto>>.Success(new PaginatedList<SaleKitDto>(result, result.Count, request.RequestData.PageIndex, request.RequestData.PageSize));
    }

    public IQueryable<SaleKitDto> HandlerFilter(SaleKitMobile_GetListWithPaginationByRoleQuery request, IQueryable<SaleKitDto> salekits)
    {
        if (request.RequestData.ParentId != null && request.RequestData.ParentId != Guid.Empty && string.IsNullOrEmpty(request.RequestData.TextSearch))
        {
            salekits = salekits.Where(s => s.ParentId == request.RequestData.ParentId);
        }

        if ((request.RequestData.ParentId == null || request.RequestData.ParentId == Guid.Empty) && string.IsNullOrEmpty(request.RequestData.TextSearch))
        {
            salekits = salekits.Where(s => s.ParentId == null);
        }

        if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
        {
            salekits = salekits.Where(s => s.Name.ToLower().Contains(request.RequestData.TextSearch.ToLower())
                                   || s.FileName.ToLower().Contains(request.RequestData.TextSearch.ToLower())
                                   || s.OriginalFileName.ToLower().Contains(request.RequestData.TextSearch.ToLower()));
        }

        salekits = salekits.Where(x => TypeOfFileAllow().Contains(x.Type));

        return salekits;
    }

    public static List<string> TypeOfFileAllow()
    {
        List<string> result = new List<string>();

        result.Add(SaleKitTypeEnum.FOLDER.ToString());

        List<string> fileAllow = new List<string>()
        {
            SaleKitFileTypeEnum.XLSX.ToString(),
            SaleKitFileTypeEnum.DOCX.ToString(),
            SaleKitFileTypeEnum.PDF.ToString()
        };

        result.AddRange(fileAllow);

        return result;
    }
}
