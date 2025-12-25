using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries
{
    public record EmployeeSalary_GetByIdQuery(Guid userId, Guid Id) : IRequest<Result<EmployeeSalaryDto>>;
    public class EmployeeSalary_GetByIdQueryHandler : IRequestHandler<EmployeeSalary_GetByIdQuery, Result<EmployeeSalaryDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public EmployeeSalary_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<EmployeeSalaryDto>> Handle(EmployeeSalary_GetByIdQuery request, CancellationToken cancellationToken)
        {
            EmployeeSalaryDto? EmployeeSalary = await (from em in _context.EmployeeSalarys
                                           where em.DeleteFlag != true && em.Id == request.Id
                                           select new EmployeeSalaryDto()
                                           {
                                               Id = em.Id,
                                               EmployeeCode = em.EmployeeCode ?? "",
                                               Month = em.Month ?? DateTime.Now.Month,
                                               Year = em.Year ?? DateTime.Now.Year,
                                               IncomeBeforeTax = em.IncomeBeforeTax ?? 0,
                                               IncomeNonTax = em.IncomeNonTax ?? 0,
                                               IncomeRecevied = em.IncomeRecevied ?? 0,
                                               IncomeTax = em.IncomeTax ?? 0,
                                               PersonalIncomeTax = em.PersonalIncomeTax ?? 0,
                                               Dependent = em.Dependent ?? 0,
                                               Insurance = em.Insurance ?? 0,
                                               RoleId = em.RoleId,
                                               UserId = em.UserId,
                                               Note = em.Note ?? "",
                                           }).AsNoTracking().FirstOrDefaultAsync();

            var eventLog = await _eventLogService.Create("EmployeeSalaryFeature", "EmployeeSalaryFeature",
                                                            "EmployeeSalary_GetByIdQuery", request.userId);
            return Result<EmployeeSalaryDto>.Success(EmployeeSalary);
        }
    }
}
