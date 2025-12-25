using OfficeOpenXml;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services.TenantService;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries;

public record EmployeeSalary_ImportSampleQuery(Guid UserId) : IRequest<ExcelPackage>;

public class EmployeeSalary_ImportSampleQueryHandler : IRequestHandler<EmployeeSalary_ImportSampleQuery, ExcelPackage>
{
    private readonly IApplicationUserService _applicationUserService;
    private readonly IApplicationRoleService _roleService;
    private readonly ITenantApplicationService _tenantApplicationService;

    public EmployeeSalary_ImportSampleQueryHandler(IApplicationUserService applicationUserService, IApplicationRoleService roleService, ITenantApplicationService tenantApplicationService)
    {
        _applicationUserService = applicationUserService;
        _roleService = roleService;
        _tenantApplicationService = tenantApplicationService;
    }

    public async Task<ExcelPackage> Handle(EmployeeSalary_ImportSampleQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Người dùng không hợp lệ");
        }

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        ExcelPackage excel = new ExcelPackage();

        #region Sheet Salary
        var salarySheet = excel.Workbook.Worksheets.Add("Danh sách thu nhập trong năm");
        salarySheet = ExcelExportHelper.GetStyle(salarySheet, 14);

        salarySheet.Cells[1, 1].Value = "Email nhân viên";
        //salarySheet.Cells[1, 2].Value = "Mã nhân viên";
        salarySheet.Cells[1, 2].Value = "Tháng";
        salarySheet.Cells[1, 3].Value = "Năm";
        salarySheet.Cells[1, 4].Value = "Trước thuế";
        salarySheet.Cells[1, 5].Value = "Không thuế";
        salarySheet.Cells[1, 6].Value = "Giảm trừ";
        salarySheet.Cells[1, 7].Value = "BHXH";
        salarySheet.Cells[1, 8].Value = "Thu nhập chịu thuế";
        salarySheet.Cells[1, 9].Value = "Thuế TNCN";
        salarySheet.Cells[1, 10].Value = "Thu nhập khác";
        salarySheet.Cells[1, 11].Value = "Thực nhận";
        salarySheet.Cells[1, 12].Value = "Mã vai trò";
        salarySheet.Cells[1, 13].Value = "Mã tổ chức";

        int currRow = 2;

        for (var index = 1; index <= 5; index++)
        {
            salarySheet.Row(currRow).Height = 20;
            salarySheet.Cells[currRow, 1].Value = $"taikhoan{index}@gmail.com";
            //salarySheet.Cells[currRow, 2].Value = $"{index}40426-ABAWWE";
            salarySheet.Cells[currRow, 2].Value = $"{index}";
            salarySheet.Cells[currRow, 3].Value = "2024";
            salarySheet.Cells[currRow, 4].Value = $"{index}00";
            salarySheet.Cells[currRow, 5].Value = $"{index}00";
            salarySheet.Cells[currRow, 6].Value = $"{index}00";
            salarySheet.Cells[currRow, 7].Value = $"{index}00";
            salarySheet.Cells[currRow, 8].Value = $"{index}00";
            salarySheet.Cells[currRow, 9].Value = $"{index}00";
            salarySheet.Cells[currRow, 10].Value = $"{index}00";
            salarySheet.Cells[currRow, 11].Value = $"{index}00";
            salarySheet.Cells[currRow, 12].Value = "Sale";
            salarySheet.Cells[currRow, 13].Value = $"dev{index}";
            currRow++;
        }

        salarySheet.Cells.AutoFitColumns();
        #endregion

        #region Sheet role
        var tenants = _tenantApplicationService.GetListTenantOfUser(request.UserId);


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
        foreach (var tenant in tenants)
        {
            tenantSheet.Row(tenantSheetRow).Height = 20;
            tenantSheet.Cells[tenantSheetRow, 1].Value = tenant.TenantId;
            tenantSheet.Cells[tenantSheetRow, 2].Value = tenant.TenantName;
            tenantSheetRow++;

            var roleSheetRow = 2;
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
