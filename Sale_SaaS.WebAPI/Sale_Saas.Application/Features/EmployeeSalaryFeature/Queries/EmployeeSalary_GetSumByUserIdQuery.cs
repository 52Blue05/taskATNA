using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries
{
    public record EmployeeSalary_GetSumByUserIdQuery(Guid userId, EmployeeSalarySumFilter req) : IRequest<Result<List<EmployeeSalaryDto>>>;

    public class EmployeeSalary_GetSumByUserIdQueryHandler : IRequestHandler<EmployeeSalary_GetSumByUserIdQuery, Result<List<EmployeeSalaryDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public EmployeeSalary_GetSumByUserIdQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<EmployeeSalaryDto>>> Handle(EmployeeSalary_GetSumByUserIdQuery request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.req.ConnectString))
            {
                _context.SetConnectString(request.req.ConnectString);
            }
            var query = _context.EmployeeSalarys.Where(x => x.DeleteFlag != true);
            if (request.req.Year > 0)
            {
                query = query.Where(x => x.Year == request.req.Year);
            }
            if (request.req.UserId != null)
            {
                query = query.Where(x => x.UserId == request.req.UserId);
            }
            var salary = query.GroupBy(s => s.User)
                              .Select(g => new EmployeeSalaryDto
                              {
                                  Id = g.Key.Id,
                                  UserId = g.Key.Id,
                                  IncomeOther = g.Sum(s => s.IncomeOther),
                                  IncomeBeforeTax = g.Sum(s => s.IncomeBeforeTax),
                                  IncomeNonTax = g.Max(s => s.IncomeNonTax),
                                  Dependent = g.Max(s => s.Dependent),
                                  Insurance = g.Max(s => s.Insurance),
                                  IncomeTax = g.Max(s => s.IncomeTax),
                                  PersonalIncomeTax = g.Max(s => s.PersonalIncomeTax),
                                  IncomeRecevied = g.Sum(s => s.IncomeRecevied)
                              });

            var result = await salary.ToListAsync();

            var eventLog = await _eventLogService.Create("EmployeeSalaryFeature", "EmployeeSalaryFeature",
                                                            "EmployeeSalary_GetSumByUserIdQuery", request.userId);

            return Result<List<EmployeeSalaryDto>>.Success(result);
        }
    }
}