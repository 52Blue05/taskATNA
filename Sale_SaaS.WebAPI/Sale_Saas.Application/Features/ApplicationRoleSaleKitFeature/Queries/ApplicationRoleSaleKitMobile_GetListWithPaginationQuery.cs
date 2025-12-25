using Sale_Saas.Application.Features.ApplicationRoleSaleKitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Role_SaleKit;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.ApplicationRoleSaleKitFeature.Queries;

public record ApplicationRoleSaleKitMobile_GetListWithPaginationQuery(Guid userId, RoleSaleKitGetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<ApplicationRoleSaleKitDto>>>;

public class ApplicationRoleSaleKitMobile_GetListWithPaginationQueryHandler : IRequestHandler<ApplicationRoleSaleKitMobile_GetListWithPaginationQuery, Result<PaginatedList<ApplicationRoleSaleKitDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IApplicationRoleService _roleService;
    private readonly IEventLogService _eventLogService;

    public ApplicationRoleSaleKitMobile_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context,
                                                                            IApplicationRoleService roleService, IEventLogService eventLogService)
    {
        _context = context;
        _mapper = mapper;
        _roleService = roleService;
        _eventLogService = eventLogService;
    }

    public async Task<Result<PaginatedList<ApplicationRoleSaleKitDto>>> Handle(ApplicationRoleSaleKitMobile_GetListWithPaginationQuery request, CancellationToken cancellationToken)
    {
        if (request.RequestData.RoleId == null)
        {
            throw new ApplicationException("Vui lòng chọn vị trí phân quyền");
        }
        var role = await _context.ApplicationRoles.FirstOrDefaultAsync(s => s.Id == request.RequestData.RoleId && !s.DeleteFlag);
        if (role == null) throw new ApplicationException($"Không tìm thấy quyền với Id: {request.RequestData.RoleId}");

        var query = _context.SaleKits
                            .Where(m => m.DeleteFlag != true)
                            .OrderByDescending(x => x.CreatedDate)
                                .Select(s => new ApplicationRoleSaleKitDto
                                {
                                    SaleKitId = s.Id,
                                    Access = false,
                                    ApplicationRoleId = (Guid)request.RequestData.RoleId,
                                    Name = s.OriginalFileName ?? "",
                                    ParentId = s.ParentId,
                                    Type = s.Type ?? "",
                                    Extension = s.Extension ?? "",
                                    CreatedDate = s.CreatedDate,
                                }).AsNoTracking().AsNoTracking();

        if (request.RequestData.ParentId != null && request.RequestData.ParentId != Guid.Empty && string.IsNullOrEmpty(request.RequestData.TextSearch))
        {
            query = query.Where(s => s.ParentId == request.RequestData.ParentId);
        }

        if ((request.RequestData.ParentId == null || request.RequestData.ParentId == Guid.Empty) && string.IsNullOrEmpty(request.RequestData.TextSearch))
        {
            query = query.Where(s => s.ParentId == null);
        }

        if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
        {
            query = query.Where(s => s.Name.ToLower().Contains(request.RequestData.TextSearch.ToLower()));
        }

        query = query.Where(x => TypeOfFileAllow().Contains(x.Type));

        int totalRecords = await query.CountAsync();

        query = query.Skip((request.RequestData.PageIndex - 1) * request.RequestData.PageSize)
                            .Take(request.RequestData.PageSize);

        var data = await query.AsNoTracking().ToListAsync();

        var ids = data.Select(s => s.SaleKitId).ToList();

        var accesses = await _context.ApplicationRole_SaleKits
                        .Where(s => s.ApplicationRoleId == request.RequestData.RoleId && s.SaleKitId != null &&
                                    ids.Contains((Guid)s.SaleKitId!) && s.DeleteFlag != true)
                        .ToListAsync();

        foreach (var item in data)
        {
            var find = accesses.Where(s => s.SaleKitId == item.SaleKitId).FirstOrDefault();
            if (find != null) item.Access = find.Access ?? false;
        }

        var eventLog = await _eventLogService.Create("ApplicationRoleSaleKitFeature", "ApplicationRoleSaleKitFeature", "ApplicationRoleSaleKitMobile_GetListWithPaginationQuery", request.userId);

        return Result<PaginatedList<ApplicationRoleSaleKitDto>>.Success(new PaginatedList<ApplicationRoleSaleKitDto>(data, totalRecords, request.RequestData.PageIndex, request.RequestData.PageSize));
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
