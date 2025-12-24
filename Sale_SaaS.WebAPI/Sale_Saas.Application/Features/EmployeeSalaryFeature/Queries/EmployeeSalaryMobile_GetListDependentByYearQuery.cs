using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services.TenantService;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries;

public record EmployeeSalaryMobile_GetListDependentByYearQuery(Guid UserId, int Year) : IRequest<Result<DependentDetailByYearDto>>;
public class EmployeeSalaryMobile_GetListDependentByYearQueryHandler : IRequestHandler<EmployeeSalaryMobile_GetListDependentByYearQuery, Result<DependentDetailByYearDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IApplicationRoleService _roleService;
    private readonly IEventLogService _eventLogService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ITenantApplicationService _tenantService;

    public EmployeeSalaryMobile_GetListDependentByYearQueryHandler(IMapper mapper, IApplicationDbContext context, IApplicationRoleService roleService, IEventLogService eventLogService, ITenantApplicationService tenantService, IApplicationUserService applicationUserService)
    {
        _context = context;
        _mapper = mapper;
        _roleService = roleService;
        _eventLogService = eventLogService;
        _tenantService = tenantService;
        _applicationUserService = applicationUserService;
    }

    public async Task<Result<DependentDetailByYearDto>> Handle(EmployeeSalaryMobile_GetListDependentByYearQuery request, CancellationToken cancellationToken)
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
                                                         Dependent = x.OrderByDescending(s => s.CreatedDate).Select(s => s.Dependent).FirstOrDefault(),
                                                         MyDependent = x.OrderByDescending(s => s.CreatedDate).Select(s => s.MyDependent).FirstOrDefault(),
                                                         NumberDependent = x.OrderByDescending(s => s.CreatedDate).Select(s => s.NumberDependent).FirstOrDefault(),
                                                         UnitDependent = x.OrderByDescending(s => s.CreatedDate).Select(s => s.UnitDependent).FirstOrDefault(),
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
                                                            Dependent = x.OrderByDescending(s => s.CreatedDate).Select(s => s.Dependent).FirstOrDefault(),
                                                            MyDependent = x.OrderByDescending(s => s.CreatedDate).Select(s => s.MyDependent).FirstOrDefault(),
                                                            NumberDependent = x.OrderByDescending(s => s.CreatedDate).Select(s => s.NumberDependent).FirstOrDefault(),
                                                            UnitDependent = x.OrderByDescending(s => s.CreatedDate).Select(s => s.UnitDependent).FirstOrDefault()
                                                        })
                                                        .ToList();

        // handle total dependent
        var listResult = listIncomeByMonthAndYearUpdated
                                                        .Select(x => new IncomeByMonthAndYearDto()
                                                        {
                                                            Month = x.Month,
                                                            Dependent = x.Dependent,
                                                            MyDependent = x.MyDependent,
                                                            NumberDependent = x.NumberDependent,
                                                            UnitDependent = x.UnitDependent,
                                                        })
                                                        .OrderBy(x => x.Month)
                                                        .ToList();

        var listDependentDetailByYear = new DependentDetailByYearDto()
        {
            nam = request.Year.ToString(),
            tongGiamTruGiaCanh = listResult.Sum(s => s.Dependent).ToString(),
            chiTietTheoThang = new List<DependentDetailByMonthDto>()
        };

        foreach (var item in listResult)
        {
            listDependentDetailByYear.chiTietTheoThang.Add(new DependentDetailByMonthDto()
            {
                thang = item.Month.ToString(),
                tong = string.IsNullOrEmpty(item.Dependent.ToString()) ? "0" : item.Dependent.ToString(),
                gtgcBanThan = string.IsNullOrEmpty(item.MyDependent.ToString()) ? "0" : item.MyDependent.ToString(),
                soNguoiPhuThuoc = string.IsNullOrEmpty(item.NumberDependent.ToString()) ? "0" : item.NumberDependent.ToString(),
                gtgcNguoiPhuThuoc = string.IsNullOrEmpty(item.UnitDependent.ToString()) ? "0" : item.UnitDependent.ToString()
            });
        }

        return Result<DependentDetailByYearDto>.Success(listDependentDetailByYear);
    }
}
