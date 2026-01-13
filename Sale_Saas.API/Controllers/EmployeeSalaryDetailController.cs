using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Commands;
using Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Dto;
using Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Queries;
using Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Requests;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.EmployeeSalary;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Services.TenantService;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeSalaryDetailController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IApplicationRoleService _roleService;
        private readonly ITenantService _tenantService;
        private readonly ILoggerService _loggerService;

        public EmployeeSalaryDetailController(IUserService customerTenantService,
                                                IApplicationUserService applicationUserService, IApplicationRoleService roleService,
                                                ITenantService tenantService, ILoggerService loggerService)
        {
            _userService = customerTenantService;
            _applicationUserService = applicationUserService;
            _roleService = roleService;
            _tenantService = tenantService;
            _loggerService = loggerService;
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromQuery] FilterQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalaryDetail_GetListQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalaryDetail_GetAllQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpPost("get-list-with-pagination")]
        public async Task<IActionResult> GetListWithPagination([FromQuery] EmployeeSalaryGetListWithPaginationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var id = GetCurrentUser() ?? Guid.Empty;
                var listUserTenant = _userService.ListAllUserTenant(id);
                var listResult = new List<EmployeeSalaryDetailDto>();
                if (listUserTenant != null)
                {
                    /*EmployeeSalaryDetailDto userInfo = null;
                    foreach (var item in listUserTenant)
                    {
                        _applicationUserService.SetConnectDB(item.ConnectString);
                        GetListWithPaginationQueryRequest requestNew = new GetListWithPaginationQueryRequest()
                        {
                            PageIndex = 1,
                            PageSize = 100,
                            UserId = item.ApplicationUserId
                        };
                        var listData = await Mediator.Send(new EmployeeSalaryAdminByRole_GetListWithPaginationQuery(requestNew));
                        foreach (var m in listData.Data.Items)
                        {
                            if (listResult.FindIndex(x => x.UserName == item.UserName) == -1)
                            {
                                userInfo = new EmployeeSalaryAllAdminRoleReportDto();
                                userInfo.ApplicationUser = m.ApplicationUser; // await _applicationUserService.GetUserBasicById(item.ApplicationUserId.Value);
                                userInfo.UserName = item.UserName;
                                userInfo.IncomeOther = m.IncomeOther;
                                userInfo.TotalIncome = m.TotalIncome;
                                userInfo.TenantNameIncomRoles = new List<TenantNameIncomRole>();
                                userInfo.TenantNameIncomRoles.Add(new TenantNameIncomRole()
                                {
                                    TenantName = item.TenantName,
                                    IncomeRoles = m.IncomeRoles
                                });
                                listResult.Add(userInfo);
                            }
                            else
                            {
                                userInfo.IncomeOther += m.IncomeOther;
                                userInfo.TotalIncome += m.TotalIncome;
                                userInfo.TenantNameIncomRoles.Add(new TenantNameIncomRole()
                                {
                                    TenantName = item.TenantName,
                                    IncomeRoles = m.IncomeRoles
                                });
                            }
                        }
                    }*/
                }
                //var result = new PaginatedList<EmployeeSalaryAllAdminRoleReportDto>(listResult, listResult.Count, request.PageIndex, request.PageSize);
                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new EmployeeSalaryDetail_GetListWithPaginationQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalaryDetail_GetByIdQuery(userId, id)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("add-or-update")]
        public async Task<IActionResult> AddOrUpdate([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalaryDetail_AddOrUpdateCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
        public async Task<IActionResult> DeleteByIds(string ids, Guid ApplicationUserId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var request = new DeleteRequest()
                {
                    Ids = ids.Split(",").ToList(),
                    ApplicationUserId = ApplicationUserId
                };

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalaryDetail_DeleteByIdCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromForm] EmployeeSalaryDetailRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Guid userId = GetCurrentUser() ?? Guid.Empty;

                string tenantId = GetCurrentTenant() ?? string.Empty;

                if (request.File != null && request.File.Length != 0)
                {
                    return Ok(await Mediator.Send(new EmployeeSalaryDetail_ImportExcelCommand(userId, request)));
                }

                return Ok(Result<string>.Failure("File import khác file mẫu!"));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] EmployeeSalaryGetListWithPaginationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                string locale = GetLocale();
                request.UserId = GetCurrentUser();
                var userId = GetCurrentUser() ?? Guid.Empty;
                var response = await Mediator.Send(new EmployeeSalaryDetail_GetListWithPaginationQuery(userId, request));
                if (response.Succeeded == false || response.Data == null)
                    throw new ApplicationException("Xuất file excel thất bại!");
                var data = response.Data.Items;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string listName = locale == LocaleEnum.vi_VN.ToString() ? "Danh sách chi tiết thu nhập theo ticket" : "List income details by tickets";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 7);

                workSheet.Cells[1, 1].Value = "NỘI DUNG";
                workSheet.Cells[1, 2].Value = "SỐ TIỀN DỰ KIẾN";
                workSheet.Cells[1, 3].Value = "SỐ TIỀN THỰC CHI";
                workSheet.Cells[1, 4].Value = "ĐỐI TƯỢNG";
                workSheet.Cells[1, 5].Value = "LOẠI CP";
                workSheet.Cells[1, 6].Value = "TÊN DỰ ÁN";
                workSheet.Cells[1, 7].Value = "THỜI ĐIỂM CHI";

                if (locale == LocaleEnum.en_US.ToString())
                {
                    workSheet.Cells[1, 2].Value = "CONTENT";
                    workSheet.Cells[1, 3].Value = "EXPECTED AMOUNT";
                    workSheet.Cells[1, 4].Value = "ACTUAL AMOUNT SPENT";
                    workSheet.Cells[1, 5].Value = "OBJECT";
                    workSheet.Cells[1, 6].Value = "TYPE NAME OF PROJECT";
                    workSheet.Cells[1, 7].Value = "TIME SPENT";
                }

                int currRow = 2;

                foreach (var item in data)
                {
                    workSheet.Row(currRow).Height = 20;
                    workSheet.Cells[currRow, 1].Value = item.Content;
                    workSheet.Cells[currRow, 2].Value = item.IncomeEta;
                    workSheet.Cells[currRow, 3].Value = item.IncomeReal;
                    workSheet.Cells[currRow, 4].Value = item.UserName;
                    workSheet.Cells[currRow, 5].Value = item.TypeCP;
                    workSheet.Cells[currRow, 6].Value = item.ProjectName;
                    //string a = item.TimeSpent.ToString();
                    //DateTime dateTime = DateTime.ParseExact(item.TimeSpent.ToString(), "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                    //workSheet.Cells[currRow, 7].Value = dateTime.ToString("dd/MM/yyyy, HH:mm:ss", CultureInfo.InvariantCulture);
                    workSheet.Cells[currRow, 7].Value = item.TimeSpent.ToString();
                    currRow++;
                }

                workSheet.Cells.AutoFitColumns();

                return File(excel.GetAsByteArray(), "application/vnd.ms-excel", String.Format("{0}.xlsx", listName));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("import-sample")]
        public async Task<IActionResult> ImportExample()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var excel = await Mediator.Send(new EmployeeSalaryDetail_ImportSampleCommand());

                return File(excel.GetAsByteArray(), "application/vnd.ms-excel", String.Format("{0}.xlsx", "Danh sách chi tiết thu nhập theo ticket"));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-user-by-id/{userId}")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await _userService.GetUserById(userId));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

    }
}
