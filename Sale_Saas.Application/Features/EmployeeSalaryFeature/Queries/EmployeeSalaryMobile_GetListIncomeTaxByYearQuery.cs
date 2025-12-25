using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services.TenantService;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries;

public record EmployeeSalaryMobile_GetListIncomeTaxByYearQuery(Guid UserId, int Year) : IRequest<Result<IncomeTaxDetailByYearDto>>;
public class EmployeeSalaryMobile_GetListIncomeTaxByYearQueryHandler : IRequestHandler<EmployeeSalaryMobile_GetListIncomeTaxByYearQuery, Result<IncomeTaxDetailByYearDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IApplicationRoleService _roleService;
    private readonly IEventLogService _eventLogService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ITenantApplicationService _tenantService;

    public EmployeeSalaryMobile_GetListIncomeTaxByYearQueryHandler(IMapper mapper, IApplicationDbContext context, IApplicationRoleService roleService, IEventLogService eventLogService, ITenantApplicationService tenantService, IApplicationUserService applicationUserService)
    {
        _context = context;
        _mapper = mapper;
        _roleService = roleService;
        _eventLogService = eventLogService;
        _tenantService = tenantService;
        _applicationUserService = applicationUserService;
    }

    public async Task<Result<IncomeTaxDetailByYearDto>> Handle(EmployeeSalaryMobile_GetListIncomeTaxByYearQuery request, CancellationToken cancellationToken)
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
                                                         IncomeTax = x.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeTax).FirstOrDefault(),
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
                                                            IncomeTax = x.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeTax).FirstOrDefault()
                                                        })
                                                        .OrderBy(x => x.Month)
                                                        .ToList();

        var listIncomeTaxByYear = new IncomeTaxDetailByYearDto()
        {
            nam = request.Year.ToString(),
            tongThuNhapChiuThue = listIncomeByMonthAndYearUpdated.Sum(s => s.IncomeTax).ToString(),
            chiTietTheoThang = new List<IncomeTaxDetailByMonthDto>()
        };

        foreach (var item in listIncomeByMonthAndYearUpdated)
        {
            listIncomeTaxByYear.chiTietTheoThang.Add(new IncomeTaxDetailByMonthDto()
            {
                thang = item.Month.ToString(),
                thuNhap = item.IncomeTax.ToString()
            });
        }

        return Result<IncomeTaxDetailByYearDto>.Success(listIncomeTaxByYear);
    }
}

