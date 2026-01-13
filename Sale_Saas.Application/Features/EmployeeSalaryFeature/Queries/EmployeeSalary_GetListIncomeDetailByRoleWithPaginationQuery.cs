using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services.TenantService;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries;

public record EmployeeSalary_GetListIncomeDetailByRoleWithPaginationQuery(GetListWithPaginationQueryRequest RequestData, string CurrentTenant) : IRequest<Result<PaginatedList<EmployeeSalaryAllAdminRoleReportDto>>>;

public class EmployeeSalary_GetListIncomeDetailByRoleWithPaginationQueryHandler : IRequestHandler<EmployeeSalary_GetListIncomeDetailByRoleWithPaginationQuery, Result<PaginatedList<EmployeeSalaryAllAdminRoleReportDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IApplicationRoleService _roleService;
    private readonly IEventLogService _eventLogService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ITenantApplicationService _tenantService;

    public EmployeeSalary_GetListIncomeDetailByRoleWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context, IApplicationRoleService roleService, IEventLogService eventLogService, ITenantApplicationService tenantService, IApplicationUserService applicationUserService)
    {
        _context = context;
        _mapper = mapper;
        _roleService = roleService;
        _eventLogService = eventLogService;
        _tenantService = tenantService;
        _applicationUserService = applicationUserService;
    }

    public async Task<Result<PaginatedList<EmployeeSalaryAllAdminRoleReportDto>>> Handle(EmployeeSalary_GetListIncomeDetailByRoleWithPaginationQuery request, CancellationToken cancellationToken)
    {
        if (request.RequestData.UserId == null)
        {
            throw new ApplicationException("Không tìm thấy dữ liệu người dùng");
        }

        var listUserTenant = _tenantService.GetAllUserOfTenant(request.CurrentTenant);

        var listResult = new List<EmployeeSalaryAllAdminRoleReportDto>();

        if (listUserTenant != null && listUserTenant.Count > 0)
        {
            EmployeeSalaryAllAdminRoleReportDto salary;

            foreach (var item in listUserTenant)
            {
                _applicationUserService.SetConnectDB(item.ConnectString ?? "");

                // initial report for each user
                if (listResult.FindIndex(x => x.UserName == item.UserName) == -1)
                {
                    var userRoles = (await _roleService.GetListRoleByUserId(item.ApplicationUserId.Value))
                                                       .Where(x => x.Name != "Admin")
                                                       .Select(s => s.DisplayName)
                                                       .ToList();

                    if (userRoles.Count == 0)
                    {
                        continue;
                    }

                    //List<TenantNameRole> listTenantNameRoles = new List<TenantNameRole>();
                    var newItem = new EmployeeSalaryAllAdminRoleReportDto()
                    {
                        UserName = item.UserName,
                        IncomeOther = 0,
                        TotalIncome = 0,
                        ApplicationUser = await _applicationUserService.GetUserBasicById(item.ApplicationUserId.Value),
                    };

                    newItem.TenantNameIncomRoles.Add(new TenantNameIncomRole()
                    {
                        TenantName = item.TenantName
                    });

                    listResult.Add(newItem);
                }

                var filter = new EmployeeSalarySumFilter()
                {
                    UserId = item.ApplicationUserId,
                    Year = request.RequestData.Time.HasValue ? request.RequestData.Time.Value.Year : DateTime.Now.Year,
                };

                var employeeSalaryInitial = listResult.Where(x => x.UserName == item.UserName).FirstOrDefault();
                var listTenantOfUser = _tenantService.GetListTenantOfUser(filter.UserId ?? Guid.Empty);
                var listTempEmployeeSalaryResult = new List<IncomeOfRoleByMonthDto>();
                var listTempHandleIncomeOtherAndIncomeReceived = new List<IncomeOfRoleByMonthDto>();

                foreach (var tenant in listTenantOfUser)
                {
                    var listEmployeeSalaryAfterCalculate = await HandleLogicEmployeeSalaryForAllTenant(filter, tenant.ConnectString ?? "", tenant.TenantId, tenant.TenantName);

                    if (listEmployeeSalaryAfterCalculate != null && listEmployeeSalaryAfterCalculate.Count > 0)
                    {
                        listTempEmployeeSalaryResult.AddRange(listEmployeeSalaryAfterCalculate);

                        var temp = listEmployeeSalaryAfterCalculate.GroupBy(x => x.Month)
                                                        .Select(x => new IncomeOfRoleByMonthDto()
                                                        {
                                                            Month = x.Key,
                                                            IncomeReceived = x.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeReceived).FirstOrDefault(),
                                                            IncomeOther = x.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeOther).FirstOrDefault(),
                                                            CreatedDate = x.OrderByDescending(s => s.CreatedDate).Select(s => s.CreatedDate).FirstOrDefault()
                                                        })
                                                        .ToList();

                        listTempHandleIncomeOtherAndIncomeReceived.AddRange(temp);
                    }
                }

                // handle calculate income other and income received => filter by month
                var listHandleIncomeOtherAndIncomeReceived = listTempHandleIncomeOtherAndIncomeReceived.GroupBy(x => x.Month)
                                                                                                       .Select(x => new IncomeOfRoleByMonthDto()
                                                                                                       {
                                                                                                           Month = x.Key,
                                                                                                           IncomeReceived = x.Sum(s => s.IncomeReceived),
                                                                                                           IncomeOther = x.OrderByDescending(s => s.CreatedDate)
                                                                                                                          .Select(s => s.IncomeOther)
                                                                                                                          .FirstOrDefault()
                                                                                                       })
                                                                                                       .ToList();

                // handle calculate role
                var listTempIncomeByRole = listTempEmployeeSalaryResult.GroupBy(x => new { x.RoleId, x.RoleName, x.TenantId, x.TenantName })
                                                                       .Select(x => new IncomeOfRoleByMonthDto()
                                                                       {
                                                                           RoleId = x.Key.RoleId,
                                                                           RoleName = x.Key.RoleName,
                                                                           TenantId = x.Key.TenantId,
                                                                           TenantName = x.Key.TenantName,
                                                                           IncomeByRole = x.Sum(x => x.IncomeByRole),
                                                                       })
                                                                       .ToList();

                var tenantIncomeRoles = listTempIncomeByRole.GroupBy(x => new { x.TenantId, x.TenantName })
                                                            .Select(g => new TenantNameIncomRole()
                                                            {
                                                                TenantName = g.Key.TenantName,
                                                                IncomeRoles = g.Select(s => new IncomeRoleDto()
                                                                {
                                                                    Id = s.RoleId ?? Guid.Empty,
                                                                    DisplayName = s.RoleName ?? "",
                                                                    Income = s.IncomeByRole
                                                                }).ToList()
                                                            })
                                                            .ToList();

                if (employeeSalaryInitial != null)
                {
                    employeeSalaryInitial.TotalIncome = listHandleIncomeOtherAndIncomeReceived.Sum(s => s.IncomeReceived);
                    employeeSalaryInitial.IncomeOther = listHandleIncomeOtherAndIncomeReceived.Sum(s => s.IncomeOther);
                    employeeSalaryInitial.TenantNameIncomRoles = tenantIncomeRoles;
                }
            }
        }

        var result = new PaginatedList<EmployeeSalaryAllAdminRoleReportDto>(listResult, listResult.Count, request.RequestData.PageIndex, request.RequestData.PageSize);

        return Result<PaginatedList<EmployeeSalaryAllAdminRoleReportDto>>.Success(result);
    }

    private async Task<List<IncomeOfRoleByMonthDto>> HandleLogicEmployeeSalaryForAllTenant(EmployeeSalarySumFilter filter, string connectionString, string tenantId, string tenantName)
    {
        _applicationUserService.SetConnectDB(connectionString);

        var query = _context.EmployeeSalarys.Where(s => s.DeleteFlag != true);

        if (filter.Year > 0)
        {
            query = query.Where(x => x.Year == filter.Year);
        }

        if (filter.UserId != null)
        {
            query = query.Where(x => x.UserId == filter.UserId);
        }

        List<IncomeOfRoleByMonthDto> listIncomeOfRoleByMonth = new List<IncomeOfRoleByMonthDto>();
        var listSalaryByRoleOfUser = await query.ToListAsync();

        if (listSalaryByRoleOfUser == null || listSalaryByRoleOfUser.Count <= 0)
        {
            return listIncomeOfRoleByMonth;
        }

        // field need to get: month, income received, income other, role id, income by role
        // if it has the same month => difference role

        foreach (var item in listSalaryByRoleOfUser)
        {

            IncomeOfRoleByMonthDto incomeOfRoleByMonthDto = new IncomeOfRoleByMonthDto()
            {
                Month = item.Month,
                RoleId = item.RoleId,
                RoleName = await _context.ApplicationRoles.Where(x => x.DeleteFlag != true && x.Id == item.RoleId).Select(x => x.Name).FirstOrDefaultAsync(),
                IncomeByRole = item.IncomeByRole,
                IncomeOther = item.IncomeOther,
                IncomeReceived = item.IncomeRecevied,
                TenantId = tenantId,
                TenantName = tenantName,
                CreatedDate = item.CreatedDate
            };

            listIncomeOfRoleByMonth.Add(incomeOfRoleByMonthDto);
        }

        listIncomeOfRoleByMonth = listIncomeOfRoleByMonth.GroupBy(x => new { x.Month, x.RoleId, x.RoleName, x.TenantId, x.TenantName })
                                                         .Select(x => new IncomeOfRoleByMonthDto()
                                                         {
                                                             Month = x.Key.Month,
                                                             RoleId = x.Key.RoleId,
                                                             RoleName = x.Key.RoleName,
                                                             IncomeByRole = x.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeByRole).FirstOrDefault(),
                                                             IncomeOther = x.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeOther).FirstOrDefault(),
                                                             IncomeReceived = x.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeReceived).FirstOrDefault(),
                                                             TenantId = x.Key.TenantId,
                                                             TenantName = x.Key.TenantName,
                                                             CreatedDate = x.OrderByDescending(s => s.CreatedDate).Select(s => s.CreatedDate).FirstOrDefault()
                                                         })
                                                         .ToList();

        return listIncomeOfRoleByMonth;
    }

}
