using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services.TenantService;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries;

public record EmployeeSalaryMobile_GetTotalIncomeCurrentUserQuery(Guid UserId, Guid GroupTenantId) : IRequest<Result<string>>;
public class EmployeeSalaryMobile_GetTotalIncomeCurrentUserQueryHandler : IRequestHandler<EmployeeSalaryMobile_GetTotalIncomeCurrentUserQuery, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IApplicationRoleService _roleService;
    private readonly IEventLogService _eventLogService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ITenantApplicationService _tenantService;

    public EmployeeSalaryMobile_GetTotalIncomeCurrentUserQueryHandler(IMapper mapper, IApplicationDbContext context, IApplicationRoleService roleService, IEventLogService eventLogService, ITenantApplicationService tenantService, IApplicationUserService applicationUserService)
    {
        _context = context;
        _mapper = mapper;
        _roleService = roleService;
        _eventLogService = eventLogService;
        _tenantService = tenantService;
        _applicationUserService = applicationUserService;
    }

    public async Task<Result<string>> Handle(EmployeeSalaryMobile_GetTotalIncomeCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy dữ liệu người dùng");
        }

        var listTenantOfGroupTenant = _tenantService.GetListTenantByGroupTenant(request.GroupTenantId);

        var listConnectionStringOfAllTenant = _tenantService.GetListConnectionStringByUserAndTenant(listTenantOfGroupTenant, request.UserId);

        List<IncomeByMonthAndYearDto> listSalary = new List<IncomeByMonthAndYearDto>();

        foreach (var connectionString in listConnectionStringOfAllTenant)
        {
            _applicationUserService.SetConnectDB(connectionString);

            // get all employee salary
            var listIncomeByMonthAndYear = await _context.EmployeeSalarys.Where(x => x.UserId == request.UserId)
                                                     .GroupBy(x => new { x.Month, x.Year })
                                                     .Select(x => new IncomeByMonthAndYearDto()
                                                     {
                                                         Month = x.Key.Month,
                                                         Year = x.Key.Year,
                                                         IncomeReceived = x.OrderByDescending(s => s.CreatedDate)
                                                                           .Select(s => s.IncomeRecevied)
                                                                           .FirstOrDefault()
                                                     })
                                                     .ToListAsync();

            listSalary.AddRange(listIncomeByMonthAndYear);

            _applicationUserService.ClearConnectDB();
        }

        int currentYear = DateTime.Now.Year;
        listSalary = listSalary.Where(x => x.Year <= currentYear).ToList();

        var totalIncomeResult = listSalary.Sum(s => s.IncomeReceived);

        return Result<string>.Success(totalIncomeResult.ToString());
    }
}
