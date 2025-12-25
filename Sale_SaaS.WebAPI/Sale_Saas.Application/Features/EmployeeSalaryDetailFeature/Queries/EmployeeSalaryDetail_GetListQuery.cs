using Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Queries
{
    public record EmployeeSalaryDetail_GetListQuery(Guid userId, FilterQueryRequest RequestData) : IRequest<Result<IEnumerable<EmployeeSalaryDetailDto>>>;

    public class EmployeeSalaryDetail_GetListQueryHandler : IRequestHandler<EmployeeSalaryDetail_GetListQuery, Result<IEnumerable<EmployeeSalaryDetailDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public EmployeeSalaryDetail_GetListQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<IEnumerable<EmployeeSalaryDetailDto>>> Handle(EmployeeSalaryDetail_GetListQuery request, CancellationToken cancellationToken)
        {
            var query = from em in _context.EmployeeSalaryDetails
                        where em.DeleteFlag != true
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
                        };
            if (request.RequestData.UserId!=null)
            {
                query = query.Where(x => x.UserId==request.RequestData.UserId);
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

            var eventLog = await _eventLogService.Create("EmployeeSalaryDetailFeature", "EmployeeSalaryDetailFeature",
               "EmployeeSalaryDetail_GetListQuery", request.userId);

            return Result<IEnumerable<EmployeeSalaryDetailDto>>.Success(data);
        }
    }
}
