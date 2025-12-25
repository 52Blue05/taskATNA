using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries
{
    public record EmployeeSalaryByRole_GetListWithPaginationQuery(Guid userId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<EmployeeSalaryRoleReportDto>>>;
    public class EmployeeSalaryByRole_GetListWithPaginationQueryHandler : IRequestHandler<EmployeeSalaryByRole_GetListWithPaginationQuery, Result<PaginatedList<EmployeeSalaryRoleReportDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IApplicationRoleService _roleService;
		private readonly IFeaturePermissionService _permissionService;
        private readonly IEventLogService _eventLogService;

        public EmployeeSalaryByRole_GetListWithPaginationQueryHandler(
            IMapper mapper, IApplicationDbContext context,IApplicationRoleService roleService, 
            IFeaturePermissionService permissionService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _roleService = roleService;
            _permissionService = permissionService;
            _eventLogService = eventLogService;
        }

        public async Task<Result<PaginatedList<EmployeeSalaryRoleReportDto>>> Handle(EmployeeSalaryByRole_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            if (request.RequestData.UserId == null)
            {
                throw new ApplicationException("Không tìm thấy dữ liệu người dùng");
            }

            //var roles = await _context.ApplicationRoles.ToListAsync();
            var emptyGuid = Guid.Empty;
            var query = _context.EmployeeSalarys.Where(m => m.DeleteFlag !=true && m.RoleId != null);
            if (request.RequestData.RoleId != null)
            {
                query = query.Where(x => x.RoleId == request.RequestData.RoleId);
            }
            if (!string.IsNullOrEmpty(request.RequestData.Code))
            {
                query = query.Where(x => x.EmployeeCode.Contains(request.RequestData.Code));
            }

			query = query.Where(x => x.UserId == request.RequestData.UserId);

			var salary = query.GroupBy(s => new { s.Month, s.Year} )
                              .Select(g => new EmployeeSalaryRoleReportDto
                              {                                 
                                  Month =g.Key.Month,
                                  Year=g.Key.Year,
                                  IncomeOther=g.Sum(x => x.IncomeOther),                                
                                  TotalIncome=g.Sum(x=>x.IncomeRecevied) + g.Sum(x => x.IncomeOther)                                  
                              });

            int totalRecords =  salary.Count();
            salary = salary.Skip((request.RequestData.PageIndex - 1) * request.RequestData.PageSize)
                         .Take(request.RequestData.PageSize);

            var data =  salary.OrderByDescending(x=>x.Year).ThenBy(x=>x.Month).ToList();
            foreach (var item in data)
            {
                item.IncomeRoles = query.Where(x => x.Month == item.Month && x.Year == item.Year).GroupBy(x => x.RoleId).Select(
                    g => new IncomeRoleDto()
                    {
                        Id=g.Key.Value,
                        Income=g.Sum(x=>x.IncomeRecevied),
                        //DisplayName=roles.Where(x=>x.Id== g.Key.Value).Select(x=>x.DisplayName).FirstOrDefault()

                    }).ToList();
            }

            var eventLog = await _eventLogService.Create("EmployeeSalaryFeature", "EmployeeSalaryFeature",
                                                            "EmployeeSalaryByRole_GetListWithPaginationQuery", request.userId);

            return Result<PaginatedList<EmployeeSalaryRoleReportDto>>.Success(new PaginatedList<EmployeeSalaryRoleReportDto>(data, totalRecords, request.RequestData.PageIndex, request.RequestData.PageSize));
        }
    }
}
