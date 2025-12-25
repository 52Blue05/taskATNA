using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services.TenantService;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries;

public record EmployeeSalaryMobile_GetListIncomeBeforeTaxByYearQuery(Guid UserId, int Year) : IRequest<Result<IncomeBeforeTaxDetailByYearDto>>;
public class EmployeeSalaryMobile_GetListIncomeBeforeTaxByYearQueryHandler : IRequestHandler<EmployeeSalaryMobile_GetListIncomeBeforeTaxByYearQuery, Result<IncomeBeforeTaxDetailByYearDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IApplicationRoleService _roleService;
    private readonly IEventLogService _eventLogService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ITenantApplicationService _tenantService;

    public EmployeeSalaryMobile_GetListIncomeBeforeTaxByYearQueryHandler(IMapper mapper, IApplicationDbContext context, IApplicationRoleService roleService, IEventLogService eventLogService, ITenantApplicationService tenantService, IApplicationUserService applicationUserService)
    {
        _context = context;
        _mapper = mapper;
        _roleService = roleService;
        _eventLogService = eventLogService;
        _tenantService = tenantService;
        _applicationUserService = applicationUserService;
    }

    public async Task<Result<IncomeBeforeTaxDetailByYearDto>> Handle(EmployeeSalaryMobile_GetListIncomeBeforeTaxByYearQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy dữ liệu người dùng");
        }

        var listTenantOfUser = _tenantService.GetListTenantOfUser(request.UserId);

        //var listConnectionStringOfAllTenant = listTenantOfUser.Select(s => s.ConnectString).ToList();

        List<IncomeOfRoleByMonthMobileDto> listSalary = new List<IncomeOfRoleByMonthMobileDto>();

        foreach (var tenant in listTenantOfUser)
        {
            if (tenant.ConnectString == null)
            {
                continue;
            }

            _applicationUserService.SetConnectDB(tenant.ConnectString);

            var userRoles = (await _roleService.GetListRoleByUserId(request.UserId))
                                               .Where(x => x.Name != "Admin" && x.DeleteFlag != true)
                                               .Select(x => new RoleOfUserMobileDto()
                                               {
                                                   Id = x.Id,
                                                   RoleName = x.DisplayName
                                               })
                                               .ToList();

            if (userRoles.Count == 0)
            {
                continue;
            }

            // get all employee salary
            var listIncomeByRoleAndMonth = await _context.EmployeeSalarys.Where(x => x.UserId == request.UserId && x.Year == request.Year && x.DeleteFlag != true)
                                                                         .GroupBy(x => new { x.Month, x.RoleId })
                                                                         .Select(x => new IncomeOfRoleByMonthMobileDto()
                                                                         {
                                                                             Month = x.Key.Month,
                                                                             RoleId = x.Key.RoleId,
                                                                             IncomeByRole = x.OrderByDescending(s => s.CreatedDate)
                                                                                             .Select(s => s.IncomeByRole)
                                                                                             .FirstOrDefault(),
                                                                             IncomeOther = x.OrderByDescending(s => s.CreatedDate)
                                                                                            .Select(s => s.IncomeOther)
                                                                                            .FirstOrDefault(),
                                                                             TenantId = tenant.TenantId,
                                                                             TenantName = tenant.TenantName,
                                                                             CreatedDate = x.OrderByDescending(s => s.CreatedDate)
                                                                                            .Select(s => s.CreatedDate)
                                                                                            .FirstOrDefault()
                                                                         })
                                                                         .ToListAsync();

            listIncomeByRoleAndMonth.ForEach(item =>
            {
                var role = userRoles.FirstOrDefault(s => s.Id == item.RoleId);
                item.RoleName = role?.RoleName ?? "";
            });

            listSalary.AddRange(listIncomeByRoleAndMonth);

            _applicationUserService.ClearConnectDB();
        }

        // handle merge between many tenant
        var listIncomeByRoleAndMonthUpdated = listSalary.GroupBy(x => new { x.Month, x.RoleName })
                                                        .Select(x => new IncomeOfRoleByMonthMobileDto()
                                                        {
                                                            Month = x.Key.Month,
                                                            RoleName = x.Key.RoleName,
                                                            IncomeByRole = x.Sum(s => s.IncomeByRole),
                                                            IncomeOther = x.OrderByDescending(s => s.CreatedDate)
                                                                           .Select(s => s.IncomeOther)
                                                                           .FirstOrDefault(),
                                                            CreatedDate = x.OrderByDescending(s => s.CreatedDate)
                                                                           .Select(s => s.CreatedDate)
                                                                           .FirstOrDefault()
                                                        })
                                                        .OrderBy(x => x.Month)
                                                        .ToList();

        var listIncomeOther = listIncomeByRoleAndMonthUpdated.GroupBy(x => x.Month)
                                                             .Select(x => new IncomeOtherByMonth()
                                                             {
                                                                 IncomeOther = x.OrderByDescending(s => s.CreatedDate)
                                                                                .Select(s => s.IncomeOther)
                                                                                .FirstOrDefault(),
                                                                 Month = x.Key
                                                             })
                                                             .ToList();

        var totalIncomeByRole = listIncomeByRoleAndMonthUpdated.Sum(s => s.IncomeByRole) ?? 0;
        var totalIncome = listIncomeOther.Sum(x => x.IncomeOther) + totalIncomeByRole;

        var listIncomeBeforeTaxByYear = new IncomeBeforeTaxDetailByYearDto()
        {
            nam = request.Year.ToString(),
            tongThuNhapTruocThue = totalIncome.ToString()
        };

        var listSalaryByMonth = listIncomeByRoleAndMonthUpdated.GroupBy(x => x.Month)
                                                               .Select(x =>
                                                               {
                                                                   var incomeOther = listIncomeOther.Where(s => s.Month == x.Key)
                                                                                                    .Select(s => s.IncomeOther)
                                                                                                    .FirstOrDefault();

                                                                   return new IncomeBeforeTaxDetailByMonthDto()
                                                                   {
                                                                       thang = x.Key.ToString(),
                                                                       thuNhap = (x.Sum(s => s.IncomeByRole) + incomeOther).ToString(),
                                                                       cacKhoanThuNhapKhac = string.IsNullOrEmpty(incomeOther.ToString())
                                                                                                        ? "0"
                                                                                                        : incomeOther.ToString(),
                                                                       chiTietVaiTro = listIncomeByRoleAndMonthUpdated
                                                                                        .Where(s => s.Month == x.Key)
                                                                                        .Select(s => new IncomeOfRoleMobileByMonthDto()
                                                                                        {
                                                                                            TenVaiTro = s.RoleName,
                                                                                            ThuNhapTheoVaiTro = string.IsNullOrEmpty(s.IncomeByRole.ToString())
                                                                                                                        ? "0"
                                                                                                                        : s.IncomeByRole.ToString()
                                                                                        })
                                                                                        .ToList(),
                                                                   };
                                                               })
                                                               .ToList();

        listIncomeBeforeTaxByYear.chiTietTheoThang = listSalaryByMonth;

        return Result<IncomeBeforeTaxDetailByYearDto>.Success(listIncomeBeforeTaxByYear);
    }
}

