using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries
{
    public record EmployeeSalary_GetListQuery(Guid userId, FilterQueryRequest RequestData) : IRequest<Result<IEnumerable<EmployeeSalaryDto>>>;

    public class EmployeeSalary_GetListQueryHandler : IRequestHandler<EmployeeSalary_GetListQuery, Result<IEnumerable<EmployeeSalaryDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public EmployeeSalary_GetListQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<IEnumerable<EmployeeSalaryDto>>> Handle(EmployeeSalary_GetListQuery request, CancellationToken cancellationToken)
        {
            var query = from em in _context.EmployeeSalarys
                        where em.DeleteFlag != true
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
                        };
            if (request.RequestData.UserId!=null)
            {
                query = query.Where(x => x.UserId==request.RequestData.UserId);
            }
            if (request.RequestData.RoleId != null)
            {
                query = query.Where(x => x.RoleId == request.RequestData.RoleId);
            }
            if (!string.IsNullOrEmpty(request.RequestData.Code))
            {
                query = query.Where(x => x.EmployeeCode.Contains(request.RequestData.Code));
            }
            if (request.RequestData.Skip != null)
            {
                query = query.Skip(request.RequestData.Skip.Value);
            }

            if (request.RequestData.TotalRecord != null)
            {
                query = query.Take(request.RequestData.TotalRecord.Value);
            }

            var data = await query.AsNoTracking().ToListAsync();

            var eventLog = await _eventLogService.Create("EmployeeSalaryFeature", "EmployeeSalaryFeature",
                                                            "EmployeeSalary_GetListQuery", request.userId);

            return Result<IEnumerable<EmployeeSalaryDto>>.Success(data);
        }
    }
}
