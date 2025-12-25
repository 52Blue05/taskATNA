using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Models.EmployeeSalary;

namespace Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Queries
{
    public record EmployeeSalaryDetail_GetListWithPaginationQuery(Guid userId, EmployeeSalaryGetListWithPaginationRequest RequestData) : IRequest<Result<PaginatedList<EmployeeSalaryDetailDto>>>;

    public class EmployeeSalaryDetail_GetListWithPaginationQueryHandler : IRequestHandler<EmployeeSalaryDetail_GetListWithPaginationQuery, Result<PaginatedList<EmployeeSalaryDetailDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public EmployeeSalaryDetail_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<PaginatedList<EmployeeSalaryDetailDto>>> Handle(EmployeeSalaryDetail_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.RequestData.ConnectString))
            {
                _context.SetConnectString(request.RequestData.ConnectString);
            }

            var query = _context.EmployeeSalaryDetails.Where(m => m.DeleteFlag != true);
            if (request.RequestData.UserId != null)
            {
                query = query.Where(x => x.UserId == request.RequestData.UserId);
            }           
            if (!string.IsNullOrEmpty(request.RequestData.Code))
            {
                query = query.Where(x => x.EmployeeCode.Contains(request.RequestData.Code));
            }
            if (request.RequestData.Year != null)
            {
                query = query.Where(x => x.Year == request.RequestData.Year);
            }
            if (request.RequestData.Month != null)
            {
                query = query.Where(x => x.Month == request.RequestData.Month);
            }

            var eventLog = await _eventLogService.Create("EmployeeSalaryDetailFeature", "EmployeeSalaryDetailFeature",
               "EmployeeSalaryDetail_GetListWithPaginationQuery", request.userId);

            return Result<PaginatedList<EmployeeSalaryDetailDto>>.Success(await query
                                         .OrderBy(x => x.TimeSpent)
                                         .ProjectTo<EmployeeSalaryDetailDto>(_mapper.ConfigurationProvider)
                                         .PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize));
        }
    }
}
