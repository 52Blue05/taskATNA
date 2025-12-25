using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.CustomerFeature.Commands;
using Sale_Saas.Application.Features.CustomerFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Authentication;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : BaseController
    {
        private readonly ILoggerService _loggerService;

        public CustomerController(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpPost("filter")]
        [HasPermission(PolicyTypes.DM_KH.SEARCH)]
        public async Task<IActionResult> Filter([FromQuery] FilterQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Customer_GetListQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-all")]
        [HasPermission(PolicyTypes.DM_KH.SEARCH)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Customer_GetAllQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpPost("get-list-with-pagination")]
        [HasPermission(PolicyTypes.DM_KH.VIEW)]
        public async Task<IActionResult> GetListWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Customer_GetListWithPaginationQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-by-id/{id}")]
        [HasPermission(PolicyTypes.DM_KH.VIEW)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Customer_GetByIdQuery(userId, id)));
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

                var user = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Customer_AddOrUpdateCommand(user,request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
        [HasPermission(PolicyTypes.DM_KH.DELETE)]
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

                /*var user = GetCurrentUser() ?? Guid.Empty;
                var access = await _permissionService.HasPermission(user, MenuEnum.DM_KH.ToString(), PermissionType.permission.Delete);
                if (access == false) throw new ApplicationException("Tài khoản không đủ quyền truy cập");*/

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Customer_DeleteByIdCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("import")]
        [HasPermission(PolicyTypes.DM_KH.IMPORT_EXCEL)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

				List<Customer> listCustomers = new List<Customer>();
                var loginUserId = GetCurrentUser() ?? Guid.Empty;
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowcount = worksheet.Dimension.Rows;    
                        var columnCount = worksheet.Dimension.Columns;
                        for(int row = 2; row <= rowcount; row++)
                        {
                            var empCustomer = new Customer();
                            empCustomer.Id = Guid.NewGuid();
                            empCustomer.DeleteFlag = false;
                            empCustomer.CreatedDate = DateTime.Now;
                            empCustomer.LastModifiedDate = DateTime.Now;
                            empCustomer.CreatedApplicationUserId = loginUserId;
                            empCustomer.LastModifiedApplicationUserId = loginUserId;
                            for(int column = 1; column <= columnCount; column++) 
                            {
                                if (worksheet.Cells[row, column].Value.ToString() != null)
                                {
                                    switch (worksheet.Cells[1, column].Value.ToString())
                                    {
                                        case "Mã KH":
                                            empCustomer.Code = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Tên khách hàng":
                                            empCustomer.Fullname = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                    }
                                }
                            }
                            listCustomers.Add(empCustomer);
                        }
                    }
                }
                if(listCustomers.Count > 0)
                {

                    return Ok(await Mediator.Send(new Customer_ImportCommand(loginUserId, listCustomers)));
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
        [HasPermission(PolicyTypes.DM_KH.EXPORT_EXCEL)]
        public async Task<IActionResult> Export([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

				string locale = GetLocale();
                request.UserId = GetCurrentUser();
                var userId = GetCurrentUser() ?? Guid.Empty;
                var response = await Mediator.Send(new Customer_GetListWithPaginationQuery(userId, request));
                if (response.Succeeded == false || response.Data == null)
                    throw new ApplicationException("Xuất file excel thất bại!");
                var data = response.Data.Items;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string listName = locale == LocaleEnum.vi_VN.ToString() ? "Danh sách khách hàng" : "List customers";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 3);

                workSheet.Cells[1, 1].Value = "ID";
                workSheet.Cells[1, 2].Value = "MÃ KH";
                workSheet.Cells[1, 3].Value = "TÊN KHÁCH HÀNG";

                if(locale == LocaleEnum.en_US.ToString())
                {
                    workSheet.Cells[1, 2].Value = "CUSTOMERS CODE";
                    workSheet.Cells[1, 3].Value = "CUSTOMERS NAME";
                }

                int currRow = 2;

                foreach (var item in data)
                {
                    workSheet.Row(currRow).Height = 20;
                    workSheet.Cells[currRow, 1].Value = item.Id;
                    workSheet.Cells[currRow, 2].Value = item.Code;
                    workSheet.Cells[currRow, 3].Value = item.Fullname;

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

        [HttpGet("export-sample")]
        [HasPermission(PolicyTypes.DM_KH.EXPORT_EXCEL)]
        public async Task<IActionResult> ExportSample()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string listName = "File import mẫu của danh sách khách hàng";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 2);

                workSheet.Cells[1, 1].Value = "Mã KH";
                workSheet.Cells[1, 2].Value = "Tên khách hàng";

                workSheet.Row(2).Height = 20;
                workSheet.Cells[2, 1].Value = "LHR";
                workSheet.Cells[2, 2].Value = "Tasha Will";

                workSheet.Row(3).Height = 20;
                workSheet.Cells[3, 1].Value = "LHR";
                workSheet.Cells[3, 2].Value = "Tasha Will";

                workSheet.Cells.AutoFitColumns();

                return File(excel.GetAsByteArray(), "application/vnd.ms-excel", String.Format("{0}.xlsx", listName));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
