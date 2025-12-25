using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Identity;
using System.Linq;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries
{
    public record EmployeeSalaryAdminByRole_GetListWithPaginationQuery(Guid userId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<EmployeeSalaryAdminRoleReportDto>>>;
    public class EmployeeSalaryAdminByRole_GetListWithPaginationQueryHandler : IRequestHandler<EmployeeSalaryAdminByRole_GetListWithPaginationQuery, Result<PaginatedList<EmployeeSalaryAdminRoleReportDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IApplicationRoleService _roleService;
        private readonly IEventLogService _eventLogService;

        public EmployeeSalaryAdminByRole_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context,
                                                                                IApplicationRoleService roleService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _roleService = roleService;
            _eventLogService = eventLogService;
        }

        public async Task<Result<PaginatedList<EmployeeSalaryAdminRoleReportDto>>> Handle(EmployeeSalaryAdminByRole_GetListWithPaginationQuery request, CancellationToken cancellationToken)
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

            if (request.RequestData.UserId!=null)
            {
                query = query.Where(x => x.UserId == request.RequestData.UserId);
            }

            if (request.RequestData.Time.HasValue)
            {
				query = query.Where(x => x.Year == request.RequestData.Time.Value.Year);
			}

            var salary = query.GroupBy(s =>s.User )
                              .Select(g => new EmployeeSalaryAdminRoleReportDto
                              {
                                  ApplicationUser = new UserBasicInfoDto
                                  {
                                      Id = g.Key.Id,
                                      LastName = g.Key.LastName ?? "",
                                      FirstName = g.Key.FirstName ?? "",
                                      FullName = g.Key.FullName ?? "",
                                      Code = g.Key.Code ?? "",
                                      Email = g.Key.Email ?? "",
                                  },
                                  IncomeOther =g.Sum(x => x.IncomeOther),                                
                                  TotalIncome=g.Sum(x=>x.IncomeRecevied) + g.Sum(x => x.IncomeOther)
                              });

            int totalRecords =  salary.Count();
            salary = salary.Skip((request.RequestData.PageIndex - 1) * request.RequestData.PageSize)
                         .Take(request.RequestData.PageSize);

            var data =  salary.OrderBy(x=>x.ApplicationUser.FullName).ToList();
            foreach (var item in data)
            {
                item.IncomeRoles = query.Where(x => x.UserId==item.ApplicationUser.Id && x.RoleId != null).GroupBy(x => x.Role).Select(
                    g => new IncomeRoleDto()
                    {
                        Id = g.Key!.Id,
                        Income = g.Sum(x=>x.IncomeRecevied),
                        DisplayName = g.Key.DisplayName ?? ""

                    }).ToList();
            }

            var eventLog = await _eventLogService.Create("EmployeeSalaryFeature", "EmployeeSalaryFeature",
                                                                "EmployeeSalaryAdminByRole_GetListWithPaginationQuery", request.userId);

            return Result<PaginatedList<EmployeeSalaryAdminRoleReportDto>>.Success(new PaginatedList<EmployeeSalaryAdminRoleReportDto>(data, totalRecords, request.RequestData.PageIndex, request.RequestData.PageSize));
        }
    }
}
