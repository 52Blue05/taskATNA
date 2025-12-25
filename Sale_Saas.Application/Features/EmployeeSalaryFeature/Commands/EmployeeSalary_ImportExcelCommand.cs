using OfficeOpenXml;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services.TenantService;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Commands;

public record EmployeeSalary_ImportExcelCommand(Guid UserId, IFormFile File) : IRequest<Result<string>>;

public class EmployeeSalary_ImportExcelCommandHandler : IRequestHandler<EmployeeSalary_ImportExcelCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IInternalService _internalService;
    private readonly IFileStorageService _storageService;
    private readonly ITenantApplicationService _tenantService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IApplicationRoleService _applicationRoleService;

    public EmployeeSalary_ImportExcelCommandHandler(IApplicationDbContext context, IEventLogService eventLogService, IInternalService internalService, IFileStorageService storageService, ITenantApplicationService tenantService, IApplicationRoleService applicationRoleService, IApplicationUserService applicationUserService)
    {
        _context = context;
        _eventLogService = eventLogService;
        _internalService = internalService;
        _storageService = storageService;
        _tenantService = tenantService;
        _applicationRoleService = applicationRoleService;
        _applicationUserService = applicationUserService;
    }

    public async Task<Result<string>> Handle(EmployeeSalary_ImportExcelCommand request, CancellationToken cancellationToken)
    {
        HashSet<int> errorRowSet = new HashSet<int>();
        List<int> errorRowList = new List<int>();
        List<EmployeeTenant> employeeTenantList = new List<EmployeeTenant>();

        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(request.File.OpenReadStream()))
        {
            var workSheet = package.Workbook.Worksheets.FirstOrDefault();

            if (workSheet == null || workSheet.Dimension.Columns < 6)
            {
                throw new ApplicationException("File Excel không hợp lệ");
            }

            for (int row = 2; row <= workSheet.Dimension.Rows; row++)
            {
                EmployeeSalary employeeSalary = new EmployeeSalary()
                {
                    DeleteFlag = false,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    CreatedApplicationUserId = request.UserId,
                    LastModifiedApplicationUserId = request.UserId
                };

                var connectionString = "";

                // Tenant
                var tenantIdCellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.TenantId].Value;
                if (tenantIdCellValue != null)
                {
                    var tenantIdString = tenantIdCellValue.ToString().Trim();
                    var connStr = await _tenantService.GetTenantById(tenantIdString);

                    if (string.IsNullOrEmpty(connStr))
                    {
                        if (errorRowSet.Add(row))
                        {
                            errorRowList.Add(row);
                        }

                        continue;
                    }

                    connectionString = connStr;
                }
                else
                {
                    if (errorRowSet.Add(row))
                    {
                        errorRowList.Add(row);
                    }

                    continue;
                }

                List<string> listRoleOfUser;

                // Email
                var emailCellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.Email].Value;
                if (emailCellValue != null)
                {
                    var emailString = emailCellValue.ToString().Trim();
                    var user = await _applicationUserService.GetUserByEmail(emailString);

                    if (user.Data != null)
                    {
                        employeeSalary.UserId = user.Data.Id;
                        employeeSalary.EmployeeCode = user.Data.Code;
                        _applicationUserService.SetConnectDB(connectionString);
                        //var listRole = await _applicationUserService.GetApplicationRolesByUserIdAsync(request.UserId);
                        var listRole = await _applicationUserService.GetApplicationRolesByUserIdAsync(employeeSalary.UserId);
                        listRoleOfUser = listRole.Select(x => x.Name).ToList();
                    }
                    else
                    {
                        if (errorRowSet.Add(row))
                        {
                            errorRowList.Add(row);
                        }

                        continue;
                    }
                }
                else
                {
                    if (errorRowSet.Add(row))
                    {
                        errorRowList.Add(row);
                    }

                    continue;
                }

                // Month
                var monthCellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.Month].Value;
                var currentMonth = DateTime.Now.Month;
                employeeSalary.Month = monthCellValue != null ? int.Parse(monthCellValue.ToString().Trim()) : currentMonth;

                // Year
                var yearCellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.Year].Value;
                var currentYear = DateTime.Now.Year;
                employeeSalary.Year = yearCellValue != null ? int.Parse(yearCellValue.ToString().Trim()) : currentYear;

                const decimal DEFAULT_INCOME = 0;

                // Cac Khoan Thu Nhap Khac (Income Other)
                var incomeOtherCellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.IncomeOther].Value;

                employeeSalary.IncomeOther = incomeOtherCellValue != null ? decimal.Parse(incomeOtherCellValue.ToString().Trim()) : DEFAULT_INCOME;

                // Tong Thu Nhap Truoc Thue (IncomeBeforeTax)
                var incomeBeforeTaxCellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.IncomeBeforeTax].Value;

                employeeSalary.IncomeBeforeTax = incomeBeforeTaxCellValue != null ? decimal.Parse(incomeBeforeTaxCellValue.ToString().Trim()) : DEFAULT_INCOME;

                // Tong Thu Nhap Khong Chiu Thue (IncomeNonTax)
                var incomeNonTaxCellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.IncomeNonTax].Value;

                employeeSalary.IncomeNonTax = incomeNonTaxCellValue != null ? decimal.Parse(incomeNonTaxCellValue.ToString().Trim()) : DEFAULT_INCOME;

                // Tong Giam Tru Gia Canh (Dependent)
                var dependentCellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.Dependent].Value;

                employeeSalary.Dependent = dependentCellValue != null ? decimal.Parse(dependentCellValue.ToString().Trim()) : DEFAULT_INCOME;

                // Tong Tien BHXH NLD Dong Thang (Insurance)
                var insuranceCellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.Insurance].Value;

                employeeSalary.Insurance = insuranceCellValue != null ? decimal.Parse(insuranceCellValue.ToString().Trim()) : DEFAULT_INCOME;

                // Tong Thu Nhap Chiu thue (IncomeTax)
                var incomeTaxCellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.IncomeTax].Value;

                employeeSalary.IncomeTax = incomeTaxCellValue != null ? decimal.Parse(incomeTaxCellValue.ToString().Trim()) : DEFAULT_INCOME;

                // Tong Thue TNCN Tam Thu (PersonalIncomeTax)
                var personalIncomeTaxCellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.PersonalIncomeTax].Value;

                employeeSalary.PersonalIncomeTax = personalIncomeTaxCellValue != null ? decimal.Parse(personalIncomeTaxCellValue.ToString().Trim()) : DEFAULT_INCOME;

                // Tong Thu Nhap Nhan Duoc (IncomeRecevied)
                var incomeReceivedCellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.IncomeReceived].Value;

                employeeSalary.IncomeRecevied = incomeReceivedCellValue != null ? decimal.Parse(incomeReceivedCellValue.ToString().Trim()) : DEFAULT_INCOME;

                // Role - Income By Role [1]
                var roleId1CellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.RoleId_1].Value;

                var incomeByRoleId1CellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.Income_1].Value;

                await HandleIncomeByRole(employeeSalary, errorRowList, errorRowSet, employeeTenantList, connectionString, row, roleId1CellValue, incomeByRoleId1CellValue, listRoleOfUser);

                // Role - Income By Role [2]
                var roleId2CellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.RoleId_2].Value;

                var incomeByRoleId2CellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.Income_2].Value;

                await HandleIncomeByRole(employeeSalary, errorRowList, errorRowSet, employeeTenantList, connectionString, row, roleId2CellValue, incomeByRoleId2CellValue, listRoleOfUser);

                // Role - Income By Role [3]
                var roleId3CellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.RoleId_3].Value;

                var incomeByRoleId3CellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.Income_3].Value;

                await HandleIncomeByRole(employeeSalary, errorRowList, errorRowSet, employeeTenantList, connectionString, row, roleId3CellValue, incomeByRoleId3CellValue, listRoleOfUser);

                // Role - Income By Role [4]
                var roleId4CellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.RoleId_4].Value;

                var incomeByRoleId4CellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.Income_4].Value;

                await HandleIncomeByRole(employeeSalary, errorRowList, errorRowSet, employeeTenantList, connectionString, row, roleId4CellValue, incomeByRoleId4CellValue, listRoleOfUser);

                // Role - Income By Role [5]
                var roleId5CellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.RoleId_5].Value;

                var incomeByRoleId5CellValue = workSheet.Cells[row, (int)EmployeeSalaryPositionImportExcelColumnEnum.Income_5].Value;

                await HandleIncomeByRole(employeeSalary, errorRowList, errorRowSet, employeeTenantList, connectionString, row, roleId5CellValue, incomeByRoleId5CellValue, listRoleOfUser);

                // If Role - error => not save this record
                if (errorRowList != null && errorRowList.Count > 0 && errorRowList[errorRowList.Count -1] == row)
                {
                    employeeTenantList.RemoveAll(x => x.EmployeeSalary.UserId == employeeSalary.UserId
                                                   && x.EmployeeSalary.EmployeeCode == employeeSalary.EmployeeCode
                                                   && x.EmployeeSalary.Month == employeeSalary.Month
                                                   && x.EmployeeSalary.Year == employeeSalary.Year);
                }
            }
        }

        await HandleEmployeeTenantList(request.UserId, employeeTenantList, cancellationToken);

        if (errorRowList.Count > 0)
        {
            var result = String.Join(", ", errorRowList);

            throw new ApplicationException(result);
        }

        return Result<string>.Success("Ok");
    }

    public async Task HandleIncomeByRole(EmployeeSalary employeeSalary, List<int> errorRowList, HashSet<int> errorRowSet, List<EmployeeTenant> employeeTenantList, string connectionString, int row, object roleIdCellValue, object incomeByRoleIdCellValue, List<string> listRoleOfUser)
    {
        if (roleIdCellValue != null && !string.IsNullOrEmpty(roleIdCellValue.ToString()) 
            && incomeByRoleIdCellValue != null && !string.IsNullOrEmpty(incomeByRoleIdCellValue.ToString()))
        {
            var roleIdString = roleIdCellValue.ToString().Trim();
            var incomeByRoleIdString = decimal.Parse(incomeByRoleIdCellValue.ToString().Trim());
            var role = await _applicationRoleService.GetByRoleName(roleIdString);

            if (role.Data != null && role.Data.Name != null && listRoleOfUser.Contains(role.Data.Name))
            {
                EmployeeSalary newEmployeeSalary = new EmployeeSalary()
                {
                    UserId = employeeSalary.UserId,
                    EmployeeCode = employeeSalary.EmployeeCode,
                    Month = employeeSalary.Month,
                    Year = employeeSalary.Year,
                    IncomeOther = employeeSalary.IncomeOther,
                    IncomeBeforeTax = employeeSalary.IncomeBeforeTax,
                    IncomeNonTax = employeeSalary.IncomeNonTax,
                    Dependent = employeeSalary.Dependent,
                    Insurance = employeeSalary.Insurance,
                    IncomeTax = employeeSalary.IncomeTax,
                    PersonalIncomeTax = employeeSalary.PersonalIncomeTax,
                    IncomeRecevied = employeeSalary.IncomeRecevied,
                    RoleId = role.Data.Id,
                    Role = role.Data,
                    IncomeByRole = incomeByRoleIdString,

                    DeleteFlag = false,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    CreatedApplicationUserId = employeeSalary.UserId,
                    LastModifiedApplicationUserId = employeeSalary.UserId,
                };

                // add list employee tenant
                employeeTenantList.Add(new EmployeeTenant()
                {
                    ConnecttionStr = connectionString,
                    EmployeeSalary = newEmployeeSalary
                });
            }
            else
            {
                if (errorRowSet.Add(row))
                {
                    errorRowList.Add(row);
                }
            }
        }
        else if ((roleIdCellValue != null && !string.IsNullOrEmpty(roleIdCellValue.ToString())) 
            || (incomeByRoleIdCellValue != null && !string.IsNullOrEmpty(incomeByRoleIdCellValue.ToString())))
        {
            if (errorRowSet.Add(row))
            {
                errorRowList.Add(row);
            }
        }

    }

    public async Task HandleEmployeeTenantList(Guid userId, List<EmployeeTenant> employeeTenantList, CancellationToken cancellationToken)
    {
        _applicationUserService.ClearConnectDB();
        List<string> connectionStrList = new List<string>();
        foreach (var employeeTenant in employeeTenantList)
        {
            if (connectionStrList.Contains(employeeTenant.ConnecttionStr))
            {
                continue;
            }
            connectionStrList.Add(employeeTenant.ConnecttionStr);
        }

        foreach (var connectionString in connectionStrList)
        {
            _applicationUserService.SetConnectDB(connectionString);
            List<EmployeeSalary> employeeTenantListByConnectionString = employeeTenantList.Where(r => r.ConnecttionStr.Contains(connectionString)).Select(r => r.EmployeeSalary).ToList();

            // handle role get wrong
            var listRoles = await _context.ApplicationRoles.ToListAsync();

            foreach (var employee in employeeTenantListByConnectionString)
            {
                var role = listRoles.FirstOrDefault(r => r.Name == employee.Role.Name);
                employee.Role = role;
                employee.RoleId = role.Id;
            }


            if (employeeTenantListByConnectionString.Count > 0)
            {
                _context.EmployeeSalarys.AddRange(employeeTenantListByConnectionString);

                await _context.SaveChangesAsync(cancellationToken);

                _applicationUserService.ClearConnectDB();
            }
        }

        var eventLog = await _eventLogService.Create("EmployeeSalaryFeature", "EmployeeSalaryFeature", "EmployeeSalary_ImportExcelCommand", userId);
    }
}