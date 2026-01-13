using OfficeOpenXml;
using Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Requests;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services.TenantService;
using Sale_Saas.Domain.Enums;
using System.Globalization;

namespace Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Commands;

public record class EmployeeSalaryDetail_ImportExcelCommand(Guid UserId, EmployeeSalaryDetailRequest RequestData) : IRequest<Result<string>>;

public class EmployeeSalaryDetail_ImportExcelCommandHandler : IRequestHandler<EmployeeSalaryDetail_ImportExcelCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IInternalService _internalService;
    private readonly IFileStorageService _storageService;
    private readonly ITenantApplicationService _tenantService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IApplicationRoleService _applicationRoleService;

    public EmployeeSalaryDetail_ImportExcelCommandHandler(IApplicationDbContext context, IEventLogService eventLogService, IInternalService internalService, IFileStorageService storageService, ITenantApplicationService tenantService, IApplicationRoleService applicationRoleService, IApplicationUserService applicationUserService)
    {
        _context = context;
        _eventLogService = eventLogService;
        _internalService = internalService;
        _storageService = storageService;
        _tenantService = tenantService;
        _applicationRoleService = applicationRoleService;
        _applicationUserService = applicationUserService;
    }

    public async Task<Result<string>> Handle(EmployeeSalaryDetail_ImportExcelCommand request, CancellationToken cancellationToken)
    {
        HashSet<int> errorRowSet = new HashSet<int>();
        List<int> errorRowList = new List<int>();
        List<EmployeeSalaryDetail> employeeSalaryDetails = new List<EmployeeSalaryDetail>();

        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        var userCode = await _applicationUserService.GetUserBasicById(request.RequestData.UserId);

        string[] formats = { "d/M/yyyy", "dd/MM/yyyy", "dd/M/yyyy", "d/MM/yyyy" };

        using (var package = new ExcelPackage(request.RequestData.File.OpenReadStream()))
        {
            var workSheet = package.Workbook.Worksheets.FirstOrDefault();

            if (workSheet == null || workSheet.Dimension.Columns < 6)
            {
                throw new ApplicationException("File Excel không hợp lệ");
            }

            for (int row = 2; row <= workSheet.Dimension.Rows; row++)
            {
                EmployeeSalaryDetail employeeSalaryDetail = new EmployeeSalaryDetail()
                {
                    EmployeeCode = userCode.Code,
                    UserId = userCode.Id,

                    DeleteFlag = false,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    CreatedApplicationUserId = request.UserId,
                    LastModifiedApplicationUserId = request.UserId
                };

                // Content
                var contentCellValue = workSheet.Cells[row, (int)EmployeeSalaryDetailPositionImportExcelColumnEnum.Content].Value;

                employeeSalaryDetail.Content = contentCellValue != null ? contentCellValue.ToString() : string.Empty;

                // Salary Estimate
                var salaryEstimateCellValue = workSheet.Cells[row, (int)EmployeeSalaryDetailPositionImportExcelColumnEnum.SalaryEstimate].Value;

                employeeSalaryDetail.IncomeEta = salaryEstimateCellValue != null ? Decimal.Parse(salaryEstimateCellValue.ToString().Trim()) : 0;

                // Salary Actual
                var salaryActualCellValue = workSheet.Cells[row, (int)EmployeeSalaryDetailPositionImportExcelColumnEnum.SalaryActual].Value;

                employeeSalaryDetail.IncomeReal = salaryActualCellValue != null ? Decimal.Parse(salaryActualCellValue.ToString().Trim()) : 0;

                // Object
                var objectCellValue = workSheet.Cells[row, (int)EmployeeSalaryDetailPositionImportExcelColumnEnum.Object].Value;

                employeeSalaryDetail.UserName = objectCellValue != null ? objectCellValue.ToString() : string.Empty;

                // Type Of Stock
                var typeOfStockCellValue = workSheet.Cells[row, (int)EmployeeSalaryDetailPositionImportExcelColumnEnum.TypeOfStock].Value;

                employeeSalaryDetail.TypeCP = typeOfStockCellValue != null ? typeOfStockCellValue.ToString() : string.Empty;

                // ProjectName
                var projectNameCellValue = workSheet.Cells[row, (int)EmployeeSalaryDetailPositionImportExcelColumnEnum.ProjectName].Value;

                employeeSalaryDetail.ProjectName = projectNameCellValue != null ? projectNameCellValue.ToString() : string.Empty;

                // Date Spent
                var dateSpentCellValue = workSheet.Cells[row, (int)EmployeeSalaryDetailPositionImportExcelColumnEnum.DateSpent].Value;

                DateTime currentDate = DateTime.Now;

                if (dateSpentCellValue == null)
                    employeeSalaryDetail.TimeSpent = currentDate;
                else if (dateSpentCellValue is DateTime dateTimeValue)
                {
                    // If the cell value is already a DateTime, use it directly
                    employeeSalaryDetail.TimeSpent = dateTimeValue;
                }
                else
                {
                    if (DateTime.TryParseExact(dateSpentCellValue.ToString().Trim(), formats, CultureInfo.InvariantCulture,
                  DateTimeStyles.None, out DateTime dateSpentValue))
                    {
                        employeeSalaryDetail.TimeSpent = dateSpentValue;
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


                if (errorRowList.Count > 0)
                {
                    var result = String.Join(", ", errorRowList);

                    throw new ApplicationException(result);
                }

                employeeSalaryDetails.Add(employeeSalaryDetail);
            };

            if (employeeSalaryDetails.Count > 0)
            {
                _context.EmployeeSalaryDetails.AddRange(employeeSalaryDetails);

                await _context.SaveChangesAsync(cancellationToken);

                _applicationUserService.ClearConnectDB();
            }

            var eventLog = await _eventLogService.Create("EmployeeSalaryDeatilFeature", "EmployeeSalaryDeatilFeature", "EmployeeSalaryDetail_ImportExcelCommand", request.UserId);

            return Result<string>.Success("Ok");

        }
    }

}
