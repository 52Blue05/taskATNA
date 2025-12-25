using Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Queries
{
    public record EmployeeSalaryDetail_GetAllQuery(Guid userId, GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<EmployeeSalaryDetailDto>>>;
    public class EmployeeSalaryDetail_GetAllQueryHandler : IRequestHandler<EmployeeSalaryDetail_GetAllQuery, Result<IEnumerable<EmployeeSalaryDetailDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public EmployeeSalaryDetail_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<IEnumerable<EmployeeSalaryDetailDto>>> Handle(EmployeeSalaryDetail_GetAllQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<EmployeeSalaryDetailDto> customers = (await (from em in _context.EmployeeSalaryDetails
                                                         where em.DeleteFlag != true
                                                         select new EmployeeSalaryDetailDto()
                                                         {
                                                            Id = em.Id,
                                                            EmployeeCode = em.EmployeeCode ?? "",
                                                            Month = em.Month ?? DateTime.Now.Month,
                                                            Year = em.Year ?? DateTime.Now.Year,
                                                            IncomeEta=em.IncomeEta ?? 0,
                                                            IncomeReal=em.IncomeReal ?? 0,
                                                            UserName=em.UserName ??"",
                                                            TypeCP=em.TypeCP ?? "",
                                                            ProjectName=em.ProjectName ?? "",
                                                            TimeSpent=em.TimeSpent ,                                                         
                                                            Note = em.Note ?? "",
                                                           
                                                         }).AsNoTracking().ToListAsync()).AsReadOnly();

            var eventLog = await _eventLogService.Create("EmployeeSalaryDetailFeature", "EmployeeSalaryDetailFeature",
               "EmployeeSalaryDetail_GetAllQuery", request.userId);

            return Result<IEnumerable<EmployeeSalaryDetailDto>>.Success(customers);
        }
    }
}
