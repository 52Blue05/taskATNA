using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Commands
{

    public record EmployeeSalary_ImportCommand(Guid userId, List<EmployeeTenant> RequestData) : IRequest<Result<List<EmployeeTenant>>>;
    public class EmployeeSalary_ImportCommandHandler : IRequestHandler<EmployeeSalary_ImportCommand, Result<List<EmployeeTenant>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IApplicationUserService _userService;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IApplicationRoleService _roleService;
        private readonly IEventLogService _eventLogService;

        public EmployeeSalary_ImportCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService,
                                                IApplicationUserService userService, 
                                                IApplicationRoleService roleService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _userService = userService;
            _roleService = roleService;
            _eventLogService = eventLogService;
        }
        public async Task<Result<List<EmployeeTenant>>> Handle(EmployeeSalary_ImportCommand request, CancellationToken cancellationToken)
        {
            List<string> listConStr = new List<string>();
            foreach(var re in request.RequestData)
            {
                if (listConStr.Contains(re.ConnecttionStr))
                {
                    continue;
                }
                listConStr.Add(re.ConnecttionStr);
            }
           

            foreach (var con in listConStr)
            {
                _userService.SetConnectDB(con);
                List<EmployeeSalary> listEmp = request.RequestData.Where(r => r.ConnecttionStr.Contains(con)).Select(r => r.EmployeeSalary).ToList();
                if(listEmp.Count > 0)
                {
                    foreach(var emp in listEmp)
                    {
                        var role = await _context.ApplicationRoles.Where(ar => ar.Name == emp.Role.Name).FirstOrDefaultAsync();
                        if(role == null)
                        {
                            var tenantId = await _userService.GetTenantIdByConnStr(con);
                            return Result<List<EmployeeTenant>>.Failure($"Vai trò {role.Name} ở mã tổ chức {tenantId} nên không thể import file này. Vui lòng liên hệ với quản trị viên");
                        }

                        var user = await _context.ApplicationUsers.Where(u => u.Id == emp.UserId).FirstOrDefaultAsync();

                        if(user == null)
                        {
                            var tenantId = await _userService.GetTenantIdByConnStr(con);
                            return Result<List<EmployeeTenant>>.Failure($"Mã nhân viên {emp.EmployeeCode} không có trong tổ chức {tenantId} nên không thể import file này. Vui lòng liên hệ với quản trị viên");
                        }

                        emp.RoleId = role.Id;
                        emp.Role = null;
                    }
                }
                _userService.ClearConnectDB();
            }

            foreach (var con in listConStr)
            {
                _userService.SetConnectDB(con);

                List<EmployeeSalary> listEmp = request.RequestData.Where(r => r.ConnecttionStr.Contains(con)).Select(r => r.EmployeeSalary).ToList();
                if (listEmp.Count > 0)
                {
                    _context.EmployeeSalarys.AddRange(listEmp);
                    await _context.SaveChangesAsync(cancellationToken);
                    _userService.ClearConnectDB();
                }
            }

            var eventLog = await _eventLogService.Create("EmployeeSalaryFeature", "EmployeeSalaryFeature",
                                                                "EmployeeSalary_ImportCommand", request.userId);

            return Result<List<EmployeeTenant>>.Success(request.RequestData);
        }
    }
}
