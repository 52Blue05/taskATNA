using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services.TenantService;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries;

public record EmployeeSalaryMobile_GetListInsuranceByYearQuery(Guid UserId, int Year) : IRequest<Result<InsuranceDetailByYearDto>>;
public class EmployeeSalaryMobile_GetListInsuranceByYearQueryHandler : IRequestHandler<EmployeeSalaryMobile_GetListInsuranceByYearQuery, Result<InsuranceDetailByYearDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IApplicationRoleService _roleService;
    private readonly IEventLogService _eventLogService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ITenantApplicationService _tenantService;

    public EmployeeSalaryMobile_GetListInsuranceByYearQueryHandler(IMapper mapper, IApplicationDbContext context, IApplicationRoleService roleService, IEventLogService eventLogService, ITenantApplicationService tenantService, IApplicationUserService applicationUserService)
    {
        _context = context;
        _mapper = mapper;
        _roleService = roleService;
        _eventLogService = eventLogService;
        _tenantService = tenantService;
        _applicationUserService = applicationUserService;
    }

    public async Task<Result<InsuranceDetailByYearDto>> Handle(EmployeeSalaryMobile_GetListInsuranceByYearQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy dữ liệu người dùng");
        }

        var listTenantOfUser = _tenantService.GetListTenantOfUser(request.UserId);

        var listConnectionStringOfAllTenant = listTenantOfUser.Select(s => s.ConnectString).ToList();

        List<IncomeByMonthAndYearDto> listSalary = new List<IncomeByMonthAndYearDto>();

        foreach (var connectionString in listConnectionStringOfAllTenant)
        {
            if (connectionString == null)
            {
                continue;
            }
            _applicationUserService.SetConnectDB(connectionString);

            // get all employee salary
            var listIncomeByMonthAndYear = await _context.EmployeeSalarys.Where(x => x.UserId == request.UserId && x.Year == request.Year)
                                                     .GroupBy(x => x.Month)
                                                     .Select(x => new IncomeByMonthAndYearDto()
                                                     {
                                                         Month = x.Key,
                                                         Insurance = x.OrderByDescending(s => s.CreatedDate).Select(s => s.Insurance).FirstOrDefault(),
                                                         AmountInsurance = x.OrderByDescending(s => s.CreatedDate).Select(s => s.AmountInsurance).FirstOrDefault(),
                                                         PercentInsurance = x.OrderByDescending(s => s.CreatedDate).Select(s => s.PercentInsurance).FirstOrDefault(),
                                                         CreatedDate = x.OrderByDescending(s => s.CreatedDate).Select(s => s.CreatedDate).FirstOrDefault()
                                                     })
                                                     .ToListAsync();

            listSalary.AddRange(listIncomeByMonthAndYear);

            _applicationUserService.ClearConnectDB();
        }

        // handle when have income by month and year
        var listIncomeByMonthAndYearUpdated = listSalary.GroupBy(x => x.Month)
                                                        .Select(x => new IncomeByMonthAndYearDto()
                                                        {
                                                            Month = x.Key,
                                                            Insurance = x.OrderByDescending(s => s.CreatedDate).Select(s => s.Insurance).FirstOrDefault(),
                                                            AmountInsurance = x.OrderByDescending(s => s.CreatedDate).Select(s => s.AmountInsurance).FirstOrDefault(),
                                                            PercentInsurance = x.OrderByDescending(s => s.CreatedDate).Select(s => s.PercentInsurance).FirstOrDefault()
                                                        })
                                                        .ToList();

        // handle total dependent
        var listResult = listIncomeByMonthAndYearUpdated
                                                        .Select(x => new IncomeByMonthAndYearDto()
                                                        {
                                                            Month = x.Month,
                                                            Insurance = x.Insurance,
                                                            AmountInsurance = x.AmountInsurance,
                                                            PercentInsurance = x.PercentInsurance,
                                                        })
                                                        .OrderBy(x => x.Month)
                                                        .ToList();

        var listInsuranceDetailByYear = new InsuranceDetailByYearDto()
        {
            nam = request.Year.ToString(),
            tongBHXHNLD = listResult.Sum(s => s.Insurance).ToString(),
            chiTietTheoThang = new List<InsuranceDetailByMonthDto>()
        };

        foreach (var item in listResult)
        {
            listInsuranceDetailByYear.chiTietTheoThang.Add(new InsuranceDetailByMonthDto()
            {
                thang = item.Month.ToString(),
                tong = string.IsNullOrEmpty(item.Insurance.ToString()) ? "0" : item.Insurance.ToString(),
                mucDongBHXH = string.IsNullOrEmpty(item.AmountInsurance.ToString()) ? "0" : item.AmountInsurance.ToString(),
                tyLeDong = string.IsNullOrEmpty(item.PercentInsurance.ToString()) ? "0" : item.PercentInsurance.ToString()
            });
        }

        return Result<InsuranceDetailByYearDto>.Success(listInsuranceDetailByYear);
    }
}
