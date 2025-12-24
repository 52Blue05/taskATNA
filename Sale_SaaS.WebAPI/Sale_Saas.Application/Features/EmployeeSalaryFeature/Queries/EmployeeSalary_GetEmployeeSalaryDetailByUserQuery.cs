using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services.TenantService;
using Sale_Saas.Domain.Entities.Tenant;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries;

public record EmployeeSalary_GetEmployeeSalaryDetailByUserQuery(GetEmployeeSalaryDetailOfUser RequestData) : IRequest<Result<List<IncomeByMonthAndYearDto>>>;

public class EmployeeSalary_GetEmployeeSalaryDetailByUserQueryHandler : IRequestHandler<EmployeeSalary_GetEmployeeSalaryDetailByUserQuery, Result<List<IncomeByMonthAndYearDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantApplicationService _tenantApplicationService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IApplicationRoleService _applicationRoleService;
    private readonly IEventLogService _eventLogService;

    public EmployeeSalary_GetEmployeeSalaryDetailByUserQueryHandler(IApplicationDbContext context, ITenantApplicationService tenantApplicationService, IApplicationUserService applicationUserService, IApplicationRoleService applicationRoleService, IEventLogService eventLogService)
    {
        _context = context;
        _tenantApplicationService = tenantApplicationService;
        _applicationUserService = applicationUserService;
        _applicationRoleService = applicationRoleService;
        _eventLogService = eventLogService;
    }

    public async Task<Result<List<IncomeByMonthAndYearDto>>> Handle(EmployeeSalary_GetEmployeeSalaryDetailByUserQuery request, CancellationToken cancellationToken)
    {
        if (request.RequestData.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy dữ liệu người dùng");
        }

        var listTenantOfUser = _tenantApplicationService.GetListTenantOfUser(request.RequestData.UserId);

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
            var listIncomeByMonthAndYear = await _context.EmployeeSalarys.Where(x => x.UserId == request.RequestData.UserId
                                                                                  && x.Year == request.RequestData.Year)
                                                                         .GroupBy(x => x.Month)
                                                                         .Select(x => new IncomeByMonthAndYearDto()
                                                                         {
                                                                             Month = x.Key,
                                                                             IncomeBeforeTax = x.OrderByDescending(s => s.CreatedDate)
                                                                                                .Select(s => s.IncomeBeforeTax)
                                                                                                .FirstOrDefault(),

                                                                             IncomeNonTax = x.OrderByDescending(s => s.CreatedDate)
                                                                                                .Select(s => s.IncomeNonTax)
                                                                                                .FirstOrDefault(),

                                                                             Dependent = x.OrderByDescending(s => s.CreatedDate)
                                                                                          .Select(s => s.Dependent)
                                                                                          .FirstOrDefault(),

                                                                             Insurance = x.OrderByDescending(s => s.CreatedDate)
                                                                                          .Select(s => s.Insurance)
                                                                                          .FirstOrDefault(),

                                                                             IncomeTax = x.OrderByDescending(s => s.CreatedDate)
                                                                                          .Select(s => s.IncomeTax)
                                                                                          .FirstOrDefault(),

                                                                             PersonalIncomeTax = x.OrderByDescending(s => s.CreatedDate)
                                                                                                  .Select(s => s.PersonalIncomeTax)
                                                                                                  .FirstOrDefault(),
                                                                             IncomeReceived = x.OrderByDescending(s => s.CreatedDate)
                                                                                               .Select(s => s.IncomeRecevied)
                                                                                               .FirstOrDefault(),

                                                                             MyDependent = x.OrderByDescending(s => s.CreatedDate)
                                                                                            .Select(s => s.MyDependent)
                                                                                            .FirstOrDefault(),

                                                                             NumberDependent = x.OrderByDescending(s => s.CreatedDate)
                                                                                                .Select(s => s.NumberDependent)
                                                                                                .FirstOrDefault(),

                                                                             UnitDependent = x.OrderByDescending(s => s.CreatedDate)
                                                                                              .Select(s => s.UnitDependent)
                                                                                              .FirstOrDefault(),

                                                                             AmountInsurance = x.OrderByDescending(s => s.CreatedDate)
                                                                                                .Select(s => s.AmountInsurance)
                                                                                                .FirstOrDefault(),

                                                                             PercentInsurance = x.OrderByDescending(s => s.CreatedDate)
                                                                                                 .Select(s => s.PercentInsurance)
                                                                                                 .FirstOrDefault(),

                                                                             CreatedDate = x.OrderByDescending(s => s.CreatedDate)
                                                                                            .Select(s => s.CreatedDate)
                                                                                            .FirstOrDefault()
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
                                                            Year = request.RequestData.Year,
                                                            IncomeBeforeTax = x.Sum(s => s.IncomeBeforeTax),
                                                            IncomeNonTax =  x.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeNonTax).FirstOrDefault(),
                                                            Dependent = x.OrderByDescending(s => s.CreatedDate).Select(s => s.Dependent).FirstOrDefault(),
                                                            Insurance =  x.OrderByDescending(s => s.CreatedDate).Select(s => s.Insurance).FirstOrDefault(),
                                                            IncomeTax = x.OrderByDescending(s => s.CreatedDate).Select(s => s.IncomeTax).FirstOrDefault(),
                                                            PersonalIncomeTax = x.Sum(s => s.PersonalIncomeTax),
                                                            IncomeReceived = x.Sum(s => s.IncomeReceived),

                                                            MyDependent =  x.OrderByDescending(s => s.CreatedDate).Select(s => s.MyDependent).FirstOrDefault(),
                                                            NumberDependent = x.OrderByDescending(s => s.CreatedDate).Select(s => s.NumberDependent).FirstOrDefault(),
                                                            UnitDependent = x.OrderByDescending(s => s.CreatedDate).Select(s => s.UnitDependent).FirstOrDefault(),
                                                            AmountInsurance = x.OrderByDescending(s => s.CreatedDate).Select(s => s.AmountInsurance).FirstOrDefault(),
                                                            PercentInsurance = x.OrderByDescending(s => s.CreatedDate).Select(s => s.PercentInsurance).FirstOrDefault()
                                                        })
                                                        .ToList();

        for (int month = 1; month <= 12; month++)
        {
            if (!listIncomeByMonthAndYearUpdated.Any(x => x.Month == month))
            {
                listIncomeByMonthAndYearUpdated.Add(new IncomeByMonthAndYearDto
                {
                    Month = month,
                    Year = request.RequestData.Year,
                    IncomeBeforeTax = 0,
                    IncomeNonTax = 0,
                    Dependent = 0,
                    Insurance = 0,
                    IncomeTax = 0,
                    PersonalIncomeTax = 0,
                    IncomeReceived = 0,

                    MyDependent = 0,
                    NumberDependent = 0,
                    UnitDependent = 0,
                    AmountInsurance = 0,
                    PercentInsurance = 0
                });
            }
        }

        listIncomeByMonthAndYearUpdated = listIncomeByMonthAndYearUpdated.OrderBy(x => x.Month).ToList();

        return Result<List<IncomeByMonthAndYearDto>>.Success(listIncomeByMonthAndYearUpdated);
    }
}
