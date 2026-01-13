using Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Queries
{
    public record EmployeeSalaryDetail_GetByIdQuery(Guid userId, Guid Id) : IRequest<Result<EmployeeSalaryDetailDto>>;
    public class EmployeeSalaryDetail_GetByIdQueryHandler : IRequestHandler<EmployeeSalaryDetail_GetByIdQuery, Result<EmployeeSalaryDetailDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public EmployeeSalaryDetail_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<EmployeeSalaryDetailDto>> Handle(EmployeeSalaryDetail_GetByIdQuery request, CancellationToken cancellationToken)
        {
            EmployeeSalaryDetailDto? EmployeeSalaryDetail = await (from em in _context.EmployeeSalaryDetails
                                           where em.DeleteFlag != true && em.Id == request.Id
                                           select new EmployeeSalaryDetailDto()
                                           {
                                               Id = em.Id,
                                               EmployeeCode = em.EmployeeCode ?? "",
                                               Month = em.Month ?? DateTime.Now.Month,
                                               Year = em.Year ?? DateTime.Now.Year,                                            
                                               IncomeEta = em.IncomeEta ?? 0,
                                               IncomeReal = em.IncomeReal ?? 0,
                                               UserName = em.UserName ?? "",
                                               TypeCP = em.TypeCP ?? "",
                                               ProjectName = em.ProjectName ?? "",
                                               TimeSpent = em.TimeSpent,
                                               Note = em.Note ?? "",
                                               UserId = em.UserId,
                                           }).AsNoTracking().FirstOrDefaultAsync();

            var eventLog = await _eventLogService.Create("EmployeeSalaryDetailFeature", "EmployeeSalaryDetailFeature",
               "EmployeeSalaryDetail_GetByIdQuery", request.userId);

            return Result<EmployeeSalaryDetailDto>.Success(EmployeeSalaryDetail);
        }
    }
}
