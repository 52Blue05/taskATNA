using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services.TenantService;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries;

public record EmployeeSalaryMobile_GetTotalIncomeCurrentUserByYearQuery(Guid UserId) : IRequest<Result<List<IncomeDetailByYearDto>>>;
public class EmployeeSalaryMobile_GetTotalIncomeCurrentUserByYearQueryHandler : IRequestHandler<EmployeeSalaryMobile_GetTotalIncomeCurrentUserByYearQuery, Result<List<IncomeDetailByYearDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IApplicationRoleService _roleService;
    private readonly IEventLogService _eventLogService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ITenantApplicationService _tenantService;

    public EmployeeSalaryMobile_GetTotalIncomeCurrentUserByYearQueryHandler(IMapper mapper, IApplicationDbContext context, IApplicationRoleService roleService, IEventLogService eventLogService, ITenantApplicationService tenantService, IApplicationUserService applicationUserService)
    {
        _context = context;
        _mapper = mapper;
        _roleService = roleService;
        _eventLogService = eventLogService;
        _tenantService = tenantService;
        _applicationUserService = applicationUserService;
    }

    public async Task<Result<List<IncomeDetailByYearDto>>> Handle(EmployeeSalaryMobile_GetTotalIncomeCurrentUserByYearQuery request, CancellationToken cancellationToken)
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
            var listIncomeByMonthAndYear = await _context.EmployeeSalarys.Where(x => x.UserId == request.UserId)
                                                     .GroupBy(x => new { x.Month, x.Year })
                                                     .Select(x => new IncomeByMonthAndYearDto()
                                                     {
                                                         Month = x.Key.Month,
                                                         Year = x.Key.Year,
                                                         IncomeBeforeTax = x.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeBeforeTax).FirstOrDefault(),
                                                         IncomeNonTax = x.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeNonTax).FirstOrDefault(),
                                                         Dependent = x.OrderByDescending(s => s.CreatedDate).Select(s => s.Dependent).FirstOrDefault(),
                                                         Insurance = x.OrderByDescending(s => s.CreatedDate).Select(s => s.Insurance).FirstOrDefault(),
                                                         IncomeTax = x.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeTax).FirstOrDefault(),
                                                         PersonalIncomeTax = x.OrderByDescending(s => s.CreatedDate).Select(s => s.PersonalIncomeTax).FirstOrDefault(),
                                                         IncomeReceived = x.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeRecevied).FirstOrDefault(),
                                                         CreatedDate = x.OrderByDescending(s => s.CreatedDate).Select(s => s.CreatedDate).FirstOrDefault()
                                                     })
                                                     .ToListAsync();

            listSalary.AddRange(listIncomeByMonthAndYear);

            _applicationUserService.ClearConnectDB();
        }

        // handle when have income by month and year
        var listIncomeByMonthAndYearUpdated = listSalary.GroupBy(x => new { x.Month, x.Year })
                                                        .Select(x => new IncomeByMonthAndYearDto()
                                                        {
                                                            Month = x.Key.Month,
                                                            Year = x.Key.Year,
                                                            IncomeBeforeTax = x.Sum(s => s.IncomeBeforeTax),
                                                            IncomeNonTax = x.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeNonTax).FirstOrDefault(),
                                                            Dependent = x.OrderByDescending(s => s.CreatedDate).Select(s => s.Dependent).FirstOrDefault(),
                                                            Insurance = x.OrderByDescending(s => s.CreatedDate).Select(s => s.Insurance).FirstOrDefault(),
                                                            IncomeTax = x.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeTax).FirstOrDefault(),
                                                            PersonalIncomeTax = x.Sum(s => s.PersonalIncomeTax),
                                                            IncomeReceived = x.Sum(s => s.IncomeReceived)
                                                        })
                                                        .ToList();

        var listSalaryByYear = listIncomeByMonthAndYearUpdated.GroupBy(x => x.Year)
                                                              .Select(x => new IncomeDetailByYearDto()
                                                              {
                                                                  nam = x.Key.ToString(),
                                                                  tongThuNhapTruocThue = x.Sum(s => s.IncomeBeforeTax).ToString(),
                                                                  tongThuNhapKhongChiuThue = x.Sum(s => s.IncomeNonTax).ToString(),
                                                                  tongGiamTruGiaCanh = x.Sum(s => s.Dependent).ToString(),
                                                                  tongBHXHNLD = x.Sum(s => s.Insurance).ToString(),
                                                                  tongThuNhapChiuThue = x.Sum(s => s.IncomeTax).ToString(),
                                                                  tongThueTNCNTamThu = x.Sum(s => s.PersonalIncomeTax).ToString(),
                                                                  tongThuNhapNhanDuoc = x.Sum(s => s.IncomeReceived).ToString()
                                                              })
                                                              .ToList();

        listSalaryByYear = listSalaryByYear.OrderBy(x => x.nam).ToList();

        return Result<List<IncomeDetailByYearDto>>.Success(listSalaryByYear);
    }
}

