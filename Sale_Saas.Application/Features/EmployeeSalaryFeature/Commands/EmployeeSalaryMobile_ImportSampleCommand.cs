using OfficeOpenXml;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services.TenantService;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Commands;

public record EmployeeSalaryMobile_ImportSampleCommand(Guid UserId) : IRequest<ExcelPackage>;

public class EmployeeSalaryMobile_ImportSampleCommandHandler : IRequestHandler<EmployeeSalaryMobile_ImportSampleCommand, ExcelPackage>
{
    private readonly IApplicationUserService _applicationUserService;
    private readonly IApplicationRoleService _roleService;
    private readonly ITenantApplicationService _userService;
    public EmployeeSalaryMobile_ImportSampleCommandHandler(IApplicationUserService applicationUserService, IApplicationRoleService roleService, ITenantApplicationService userService)
    {
        _applicationUserService = applicationUserService;
        _roleService = roleService;
        _userService = userService;
    }

    public async Task<ExcelPackage> Handle(EmployeeSalaryMobile_ImportSampleCommand request, CancellationToken cancellationToken)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        ExcelPackage excel = new ExcelPackage();

        #region Sheet Salary
        var salarySheet = excel.Workbook.Worksheets.Add("Danh sách thu nhập trong năm");
        salarySheet = ExcelExportHelper.GetStyle(salarySheet, 14);

        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.TenantId].Value = "Mã tổ chức";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.Email].Value = "Email nhân viên";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.Month].Value = "Tháng";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.Year].Value = "Năm";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.RoleId_1].Value = "Mã vị trí 1";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.Income_1].Value = "Thu nhập theo vị trí 1";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.RoleId_2].Value = "Mã vị trí 2";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.Income_2].Value = "Thu nhập theo vị trí 2";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.RoleId_3].Value = "Mã vị trí 3";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.Income_3].Value = "Thu nhập theo vị trí 3";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.RoleId_4].Value = "Mã vị trí 4";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.Income_4].Value = "Thu nhập theo vị trí 4";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.RoleId_5].Value = "Mã vị trí 5";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.Income_5].Value = "Thu nhập theo vị trí 5";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.IncomeOther].Value = "Các khoản thu nhập khác";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.IncomeBeforeTax].Value = "Tổng thu nhập trước thuế";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.IncomeNonTax].Value = "Tổng thu nhập không chịu thuế";

        // additional column - dependent
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.MyDependent].Value = "Tổng giảm trừ gia cảnh bản thân";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.NumberDependent].Value = "Tổng số người phụ thuộc";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.UnitDependent].Value = "Tổng giảm trừ gia cảnh người phụ thuộc";

        // additional column - insurance
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.AmountInsurance].Value = "Mức đóng BHXH NLD đóng tháng";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.PercentInsurance].Value = "Tỷ lệ đóng BHXH NLD đóng tháng (%) - (Từ 0 đến 100)";

        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.IncomeTax].Value = "Tổng thu nhập chịu thuế";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.PersonalIncomeTax].Value = "Tổng thuế TNCN tạm thu";
        salarySheet.Cells[1, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.IncomeReceived].Value = "Tổng thu nhập nhận được";

        int currRow = 2;

        int currentMonth = DateTime.Now.Month;
        int currentYear = DateTime.Now.Year;

        for (var index = 1; index <= 5; index++)
        {
            salarySheet.Row(currRow).Height = 20;
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.TenantId].Value = $"dev{index}";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.Email].Value = $"taikhoan{index}@gmail.com";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.Month].Value = $"{currentMonth}";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.Year].Value = $"{currentYear}";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.RoleId_1].Value = $"Sale";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.Income_1].Value = $"{index}00";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.RoleId_2].Value = $"Supplier";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.Income_2].Value = $"{index}00";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.RoleId_3].Value = $"";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.Income_3].Value = $"";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.RoleId_4].Value = $"";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.Income_4].Value = $"";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.RoleId_5].Value = $"";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.Income_5].Value = $"";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.IncomeOther].Value = $"{index}00";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.IncomeBeforeTax].Value = $"{index}00";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.IncomeNonTax].Value = $"{index}00";

            // dependent
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.MyDependent].Value = $"{index}00";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.NumberDependent].Value = $"{index}00";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.UnitDependent].Value = $"{index}00";

            // insurance
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.AmountInsurance].Value = $"{index}00";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.PercentInsurance].Value = $"{index}0";

            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.IncomeTax].Value = $"{index}00";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.PersonalIncomeTax].Value = $"{index}00";
            salarySheet.Cells[currRow, (int)EmployeeSalaryMobilePositionImportExcelColumnEnum.IncomeReceived].Value = $"{index}00";

            currRow++;
        }

        salarySheet.Cells.AutoFitColumns();
        #endregion

        #region Sheet role
        var tenants = _userService.GetListTenantOfUser(request.UserId);

        var roleSheet = excel.Workbook.Worksheets.Add("Danh sách vị trí");
        roleSheet = ExcelExportHelper.GetStyle(roleSheet, 4);

        roleSheet.Cells[1, 1].Value = "Mã vị trí";
        roleSheet.Cells[1, 2].Value = "Tên vị trí";
        roleSheet.Cells[1, 3].Value = "Ghi chú";
        roleSheet.Cells[1, 4].Value = "Mã tổ chức";

        var tenantSheet = excel.Workbook.Worksheets.Add("Danh sách tổ chức");
        tenantSheet = ExcelExportHelper.GetStyle(tenantSheet, 2);
        tenantSheet.Cells[1, 1].Value = "Mã tổ chức";
        tenantSheet.Cells[1, 2].Value = "Tên tổ chức";

        var tenantSheetRow = 2;
        var roleSheetRow = 2;
        foreach (var tenant in tenants)
        {
            tenantSheet.Row(tenantSheetRow).Height = 20;
            tenantSheet.Cells[tenantSheetRow, 1].Value = tenant.TenantId;
            tenantSheet.Cells[tenantSheetRow, 2].Value = tenant.TenantName;
            tenantSheetRow++;

            if (!string.IsNullOrEmpty(tenant.ConnectString))
            {
                _applicationUserService.SetConnectDB(tenant.ConnectString);
                var roles = await _roleService.GetAllQuery(new GetAllQueryRequest());
                if (roles.Data != null)
                {
                    foreach (var item in roles.Data)
                    {
                        roleSheet.Row(roleSheetRow).Height = 20;
                        roleSheet.Cells[roleSheetRow, 1].Value = item.Name;
                        roleSheet.Cells[roleSheetRow, 2].Value = item.DisplayName;
                        roleSheet.Cells[roleSheetRow, 3].Value = tenant.TenantName;
                        roleSheet.Cells[roleSheetRow, 4].Value = tenant.TenantId;
                        roleSheetRow++;
                    }
                }

            }
        }

        roleSheet.Cells.AutoFitColumns();
        tenantSheet.Cells.AutoFitColumns();
        #endregion

        return excel;
    }
}
