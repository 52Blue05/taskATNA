using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;

namespace Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Commands
{
    public record EmployeeSalaryDetail_ImportCommand(Guid userId, List<EmployeeSalaryDetailTenant> RequestData) : IRequest<Result<List<EmployeeSalaryDetailTenant>>>;
    public class EmployeeSalaryDetail_ImportCommandHandler : IRequestHandler<EmployeeSalaryDetail_ImportCommand, Result<List<EmployeeSalaryDetailTenant>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IApplicationUserService _userService;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IEventLogService _eventLogService;

        public EmployeeSalaryDetail_ImportCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService,
                                                IApplicationUserService userService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _userService = userService;
            _eventLogService = eventLogService;
        }
        public async Task<Result<List<EmployeeSalaryDetailTenant>>> Handle(EmployeeSalaryDetail_ImportCommand request, CancellationToken cancellationToken)
        {
            List<string> listConStr = new List<string>();
            foreach (var re in request.RequestData)
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

                List<EmployeeSalaryDetail> listEmp = request.RequestData.Where(r => r.ConnecttionStr.Contains(con)).Select(r => r.EmployeeSalaryDetail).ToList();

                if (listEmp.Count > 0)
                {
                    foreach (var emp in listEmp)
                    {
                        var user = await _context.ApplicationUsers.Where(u => u.Id == emp.UserId).FirstOrDefaultAsync();

                        var tenantId = await _userService.GetTenantIdByConnStr(con);

                        if(user == null) {
                            throw new Exception($"Mã nhân viên {emp.EmployeeCode} không có trong mã tổ chức {tenantId} nên không thể import file này. Vui lòng liên hệ với quản trị viên");
                        }
                    }
                    _userService.ClearConnectDB();
                }
            }

            foreach (var con in listConStr)
            {
                _userService.SetConnectDB(con);

                List<EmployeeSalaryDetail> listEmp = request.RequestData.Where(r => r.ConnecttionStr.Contains(con)).Select(r => r.EmployeeSalaryDetail).ToList();

                if (listEmp.Count > 0)
                {
                    _context.EmployeeSalaryDetails.AddRange(listEmp);
                    await _context.SaveChangesAsync(cancellationToken);
                    _userService.ClearConnectDB();
                }
            }

            var eventLog = await _eventLogService.Create("EmployeeSalaryDetailFeature", "EmployeeSalaryDetailFeature",
               "EmployeeSalaryDetail_ImportCommand", request.userId);

            return Result<List<EmployeeSalaryDetailTenant>>.Success(request.RequestData);
        }
    }
}
