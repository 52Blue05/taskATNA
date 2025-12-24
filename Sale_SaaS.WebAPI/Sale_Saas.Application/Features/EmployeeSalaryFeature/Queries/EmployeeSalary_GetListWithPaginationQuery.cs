using Sale_Saas.Application.Common.Interfaces;
using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Identity;
using System.Data;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries
{
    public record EmployeeSalary_GetListWithPaginationQuery(Guid userId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<EmployeeSalaryReportDto>>>;

    public class EmployeeSalary_GetListWithPaginationQueryHandler : IRequestHandler<EmployeeSalary_GetListWithPaginationQuery, Result<PaginatedList<EmployeeSalaryReportDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IApplicationRoleService _roleService;
        private readonly IEventLogService _eventLogService;

        public EmployeeSalary_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context,
                                                                    IApplicationRoleService roleService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _roleService = roleService;
            _eventLogService = eventLogService;
        }

        public async Task<Result<PaginatedList<EmployeeSalaryReportDto>>> Handle(EmployeeSalary_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            if(request.RequestData.UserId == null)
            {
                throw new ApplicationException("Không tìm thấy dữ liệu người dùng");
            }
            if(!string.IsNullOrEmpty(request.RequestData.ConnectString))
            {               
                _context.SetConnectString( request.RequestData.ConnectString);
            }
            var roles = await _context.ApplicationRoles.ToListAsync();
            var emptyGuid = Guid.Empty;
            var query = _context.EmployeeSalarys.Where(m => m.DeleteFlag != true && m.RoleId != null);

            if (request.RequestData.RoleId != null)
            {
                query = query.Where(x => x.RoleId == request.RequestData.RoleId);
            }
            if (!string.IsNullOrEmpty(request.RequestData.Code))
            {
                query = query.Where(x => x.EmployeeCode.Contains(request.RequestData.Code));
            }

            var userRoles = (await _roleService.GetListRoleByUserId((Guid)request.RequestData.UserId)).Select(s => s.Name).ToList();
            if (userRoles.Count == 0)
                throw new ApplicationException("Người dùng không có quyền truy cập");

            if (!userRoles.Contains("Admin"))
            {
                query = query.Where(x => x.UserId == request.RequestData.UserId);
            }

            var salary = query.GroupBy(s => s.User)
                              .Select(g => new EmployeeSalaryReportDto
                              {
                                  Id=g.Key.Id,
                                  ApplicationUser = new UserBasicInfoDto
                                  {
                                      Id = g.Key.Id,
                                      LastName = g.Key.LastName ?? "",
                                      FirstName = g.Key.FirstName ?? "",
                                      FullName = g.Key.FullName ?? "",
                                      Code=g.Key.Code
                                  },
                                  IncomeBeforeTax = g.Sum(s => s.IncomeBeforeTax),
                                  IncomeNonTax = g.Sum(s => s.IncomeNonTax),
                                  Dependent = g.Sum(s => s.Dependent),
                                  Insurance = g.Sum(s => s.Insurance),
                                  IncomeTax = g.Sum(s => s.IncomeTax),
                                  PersonalIncomeTax = g.Sum(s => s.PersonalIncomeTax),
                                  IncomeRecevied = g.Sum(s => s.IncomeRecevied),
                                  Roles = g.Select(s => new ApplicationRoleDto
                                  {
                                      Id = s.RoleId ?? emptyGuid
                                  }).ToList()
                              });

            int totalRecords = await salary.CountAsync();
            salary = salary.Skip((request.RequestData.PageIndex - 1) * request.RequestData.PageSize)
                         .Take(request.RequestData.PageSize);

            var data = await salary.ToListAsync();
            foreach (var item in data)
            {
                var RoleIds = item.Roles.Select(s => s.Id).Distinct().ToList();
                item.Roles = roles.Where(s => RoleIds.Contains(s.Id)).Select(s => new ApplicationRoleDto
                {
                    Id = s.Id,
                    Name = s.Name ?? "",
                    DisplayName = s.DisplayName ?? "",
                    Description = s.Description ?? ""
                }).ToList();
            }

            var eventLog = await _eventLogService.Create("EmployeeSalaryFeature", "EmployeeSalaryFeature",
                                                                "EmployeeSalary_GetListWithPaginationQuery", request.userId);

            return Result<PaginatedList<EmployeeSalaryReportDto>>.Success(new PaginatedList<EmployeeSalaryReportDto>(data, totalRecords, request.RequestData.PageIndex, request.RequestData.PageSize));
        }
    }
}
