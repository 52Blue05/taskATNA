using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services.TenantService;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries;

public record EmployeeSalary_GetIncomeInformationByUserQuery(GetListWithPaginationQueryRequest RequestData, string CurrentTenant) : IRequest<Result<PaginatedList<EmployeeSalaryUserReportWithAllTenantDto>>>;

public class EmployeeSalary_GetIncomeInformationByUserQueryHandler : IRequestHandler<EmployeeSalary_GetIncomeInformationByUserQuery, Result<PaginatedList<EmployeeSalaryUserReportWithAllTenantDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IApplicationRoleService _roleService;
    private readonly IEventLogService _eventLogService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ITenantApplicationService _tenantService;

    public EmployeeSalary_GetIncomeInformationByUserQueryHandler(IMapper mapper, IApplicationDbContext context, IApplicationRoleService roleService, IEventLogService eventLogService, ITenantApplicationService tenantService, IApplicationUserService applicationUserService)
    {
        _context = context;
        _mapper = mapper;
        _roleService = roleService;
        _eventLogService = eventLogService;
        _tenantService = tenantService;
        _applicationUserService = applicationUserService;
    }

    public async Task<Result<PaginatedList<EmployeeSalaryUserReportWithAllTenantDto>>> Handle(EmployeeSalary_GetIncomeInformationByUserQuery request, CancellationToken cancellationToken)
    {
        if (request.RequestData.UserId == null)
        {
            throw new ApplicationException("Không tìm thấy dữ liệu người dùng");
        }

        var listUserTenant = _tenantService.GetAllUserOfTenant(request.CurrentTenant);

        listUserTenant = listUserTenant.Where(x => x.ApplicationUserId == request.RequestData.UserId).ToList();

        var listResult = new List<EmployeeSalaryUserReportWithAllTenantDto>();

        if (listUserTenant != null && listUserTenant.Count > 0)
        {
            EmployeeSalaryUserReportWithAllTenantDto salary;

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
                    List<TenantNameRole> listTenantNameRoles = new List<TenantNameRole>();
                    var newItem = new EmployeeSalaryUserReportWithAllTenantDto()
                    {
                        UserName = item.UserName,
                        IncomeOther = 0,
                        IncomeBeforeTax = 0,
                        IncomeNonTax = 0,
                        Dependent = 0,
                        Insurance = 0,
                        IncomeTax = 0,
                        PersonalIncomeTax = 0,
                        IncomeRecevied = 0,
                        IncomeByRole = 0,
                        ApplicationUser = await _applicationUserService.GetUserBasicById(item.ApplicationUserId.Value),
                    };

                    newItem.TenantNameRoles.Add(new TenantNameRole()
                    {
                        TenantName = item.TenantName,
                        RoleNames = userRoles
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
                var listTempEmployeeSalaryResult = new List<EmployeeSalaryUserReportWithAllTenantDto>();

                foreach (var tenant in listTenantOfUser)
                {
                    var listEmployeeSalaryAfterCalculate = await HandleLogicEmployeeSalaryForAllTenant(filter, tenant.ConnectString ?? "");
                    if (listEmployeeSalaryAfterCalculate != null && listEmployeeSalaryAfterCalculate.Count > 0)
                    {
                        listTempEmployeeSalaryResult.AddRange(listEmployeeSalaryAfterCalculate);
                    }
                }

                // handle list temp employee salary result
                var listEmployeeSalaryResult = listTempEmployeeSalaryResult
                                        .GroupBy(x => new { x.Month, x.Year })
                                        .Select(g => new EmployeeSalaryUserReportWithAllTenantDto()
                                        {
                                            Month = g.Key.Month,
                                            Year = g.Key.Year,
                                            IncomeOther = g.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeOther).FirstOrDefault(),
                                            IncomeBeforeTax = g.Sum(s => s.IncomeBeforeTax),
                                            IncomeNonTax = g.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeNonTax).FirstOrDefault(),
                                            Dependent = g.OrderByDescending(s => s.CreatedDate).Select(s => s.Dependent).FirstOrDefault(),
                                            Insurance = g.OrderByDescending(s => s.CreatedDate).Select(s => s.Insurance).FirstOrDefault(),
                                            IncomeTax = g.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeTax).FirstOrDefault(),
                                            PersonalIncomeTax = g.Sum(s => s.PersonalIncomeTax),
                                            IncomeRecevied = g.Sum(s => s.IncomeRecevied)
                                        })
                                        .ToList();

                // handle final result
                if (employeeSalaryInitial != null)
                {
                    employeeSalaryInitial.IncomeOther = listEmployeeSalaryResult.Sum(x => x.IncomeOther);
                    employeeSalaryInitial.IncomeBeforeTax = listEmployeeSalaryResult.Sum(x => x.IncomeBeforeTax);
                    employeeSalaryInitial.IncomeNonTax = listEmployeeSalaryResult.Sum(x => x.IncomeNonTax);
                    employeeSalaryInitial.Dependent = listEmployeeSalaryResult.Sum(x => x.Dependent);
                    employeeSalaryInitial.Insurance = listEmployeeSalaryResult.Sum(x => x.Insurance);
                    employeeSalaryInitial.IncomeTax = listEmployeeSalaryResult.Sum(x => x.IncomeTax);
                    employeeSalaryInitial.PersonalIncomeTax = listEmployeeSalaryResult.Sum(x => x.PersonalIncomeTax);
                    employeeSalaryInitial.IncomeRecevied = listEmployeeSalaryResult.Sum(x => x.IncomeRecevied);
                }
            }

        }

        var result = new PaginatedList<EmployeeSalaryUserReportWithAllTenantDto>(listResult, listResult.Count, request.RequestData.PageIndex, request.RequestData.PageSize);

        var eventLog = await _eventLogService.Create("EmployeeSalaryFeature", "EmployeeSalaryFeature",
                                                            "EmployeeSalary_GetListWithPaginationQuery", request.RequestData.UserId);

        return Result<PaginatedList<EmployeeSalaryUserReportWithAllTenantDto>>.Success(result);
    }

    public async Task<List<EmployeeSalaryUserReportWithAllTenantDto>> HandleLogicEmployeeSalaryForAllTenant(EmployeeSalarySumFilter filter, string connectionString)
    {
        _applicationUserService.SetConnectDB(connectionString);

        var query = _context.EmployeeSalarys.Where(x => x.DeleteFlag != true);

        if (filter.Year > 0)
        {
            query = query.Where(x => x.Year == filter.Year);
        }

        if (filter.UserId != null)
        {
            query = query.Where(x => x.UserId == filter.UserId);
        }

        var employeeSalaryOfUser = await query
            .GroupBy(x => new { x.Month, x.Year })
            .Select(x => new EmployeeSalaryUserReportWithAllTenantDto
            {
                Month = x.Key.Month,
                Year = x.Key.Year,
                IncomeOther = x.OrderByDescending(s => s.CreatedDate).Select(x => x.IncomeOther).FirstOrDefault(),
                IncomeBeforeTax = x.OrderByDescending(s => s.CreatedDate).Select(x => x.IncomeBeforeTax).FirstOrDefault(),
                IncomeNonTax = x.OrderByDescending(s => s.CreatedDate).Select(x => x.IncomeNonTax).FirstOrDefault(),
                Dependent = x.OrderByDescending(s => s.CreatedDate).Select(x => x.Dependent).FirstOrDefault(),
                Insurance = x.OrderByDescending(s => s.CreatedDate).Select(x => x.Insurance).FirstOrDefault(),
                IncomeTax = x.OrderByDescending(s => s.CreatedDate).Select(x => x.IncomeTax).FirstOrDefault(),
                PersonalIncomeTax = x.OrderByDescending(s => s.CreatedDate).Select(x => x.PersonalIncomeTax).FirstOrDefault(),
                IncomeRecevied = x.OrderByDescending(s => s.CreatedDate).Select(x => x.IncomeRecevied).FirstOrDefault(),
                CreatedDate = x.OrderByDescending(s => s.CreatedDate).Select(x => x.CreatedDate).FirstOrDefault()
            })
            .ToListAsync();

        _applicationUserService.ClearConnectDB();

        return employeeSalaryOfUser;
    }
}
