using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.ContractFeature.Commands;
using Sale_Saas.Application.Features.ContractFeature.Queries;
using Sale_Saas.Application.Features.ContractStatusFeature.Queries;
using Sale_Saas.Application.Features.CustomerFeature.Queries;
using Sale_Saas.Application.Features.RelationshipStatusFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Authentication;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : BaseController
    {
        private readonly ILoggerService _loggerService;

        public ContractController(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpPost("filter")]
        [HasPermission(PolicyTypes.DM_HD.SEARCH)]
        public async Task<IActionResult> Filter([FromQuery] FilterQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Contract_GetListQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-all")]
		[HasPermission(PolicyTypes.DM_HD.SEARCH)]
		public async Task<IActionResult> GetAll([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Contract_GetAllQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpPost("get-list-with-pagination")]
		[HasPermission(PolicyTypes.DM_HD.VIEW)]
		public async Task<IActionResult> GetListWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Contract_GetListWithPaginationQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-by-id/{id}")]
		[HasPermission(PolicyTypes.DM_HD.VIEW)]
		public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Contract_GetByIdQuery(userId, id)));
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

                return Ok(await Mediator.Send(new Contract_AddOrUpdateCommand(user,request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
		[HasPermission(PolicyTypes.DM_HD.DELETE)]
		public async Task<IActionResult> DeleteByIds(string ids, Guid ApplicationUserId, string? Locale = "vi_VN")
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
                    ApplicationUserId = ApplicationUserId,
                    Locale = Locale
                };

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Contract_DeleteByIdCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("import-excel")]
		[HasPermission(PolicyTypes.DM_HD.IMPORT_EXCEL)]
		public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

				List<Contract> listContracts = new List<Contract>();
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
                        for (int row = 2; row <= rowcount; row++)
                        {
                            var empcontract = new Contract();
                            empcontract.Id = Guid.NewGuid();
                            empcontract.DeleteFlag = false;
                            empcontract.CreatedDate = DateTime.Now;
                            empcontract.LastModifiedDate = DateTime.Now;
                            empcontract.CreatedApplicationUserId = loginUserId;
                            empcontract.LastModifiedApplicationUserId = loginUserId;
                            for (int column = 1; column <= columnCount; column++)
                            {
                                if (worksheet.Cells[row, column].Value.ToString() != null)
                                {
                                    switch (worksheet.Cells[1, column].Value.ToString())
                                    {
                                        case "Mã HD":
                                            empcontract.Code = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Số HD":
                                            empcontract.Number = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Tên HD":
                                            empcontract.Name = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Ngày bắt đầu":
                                            empcontract.StartDate = DateTime.Parse(worksheet.Cells[row, column].Value.ToString() ?? DateTime.Now.ToString());
                                            break;
                                        case "Ngày kết thúc":
                                            empcontract.EndDate = DateTime.Parse(worksheet.Cells[row, column].Value.ToString() ?? DateTime.Now.ToString());
                                            break;
                                        case "Mã KH":
                                            var customer = await Mediator.Send(new Customer_GetByCodeQuery(loginUserId, worksheet.Cells[row, column].Value.ToString() ?? ""));
                                            if(customer.Data == null)
                                                return Ok(Result<string>.Failure("Mã KH khác mã KH mẫu!"));
                                            empcontract.CustomerId = customer.Data.Id;
                                            break;
                                        case "Trạng thái":
                                            var tempContractStatus = await Mediator.Send(new ContractStatus_GetByNameQuery(loginUserId, worksheet.Cells[row, column].Value.ToString() ?? ""));  
                                            if (tempContractStatus.Data == null)
                                                return Ok(Result<string>.Failure("Trạng thái của hợp đồng khác trạng thái mẫu!"));
                                            empcontract.ContractStatusId = tempContractStatus.Data.Id;
                                            break;
                                    }
                                }
                            }
                            listContracts.Add(empcontract);
                        }
                    }
                }
                if (listContracts.Count > 0)
                {
                    var userId = GetCurrentUser() ?? Guid.Empty;
                    return Ok(await Mediator.Send(new Contract_ImportCommand(userId, listContracts)));
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
		[HasPermission(PolicyTypes.DM_HD.EXPORT_EXCEL)]
		public async Task<IActionResult> Export([FromQuery] GetListWithPaginationQueryRequest request)
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
                var response = await Mediator.Send(new Contract_GetListWithPaginationQuery(userId, request));
                if (response.Succeeded == false || response.Data == null)
                    throw new ApplicationException("Xuất file excel thất bại!");
                var data = response.Data.Items;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string listName = locale == LocaleEnum.vi_VN.ToString() ? "Danh sách hợp đồng" : "List contracts";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 8);

                workSheet.Cells[1, 1].Value = "ID";
                workSheet.Cells[1, 2].Value = "MÃ HD";
                workSheet.Cells[1, 3].Value = "SỐ HD";
                workSheet.Cells[1, 4].Value = "TÊN HD";
                workSheet.Cells[1, 5].Value = "NGÀY BẮT ĐẦU";
                workSheet.Cells[1, 6].Value = "NGÀY KẾT THÚC";
                workSheet.Cells[1, 7].Value = "MÃ KH";
                workSheet.Cells[1, 8].Value = "TRẠNG THÁI";

                if (locale == LocaleEnum.en_US.ToString())
                {
                    workSheet.Cells[1, 2].Value = "CONTRACT ID";
                    workSheet.Cells[1, 3].Value = "CONTRACT NUMBER";
                    workSheet.Cells[1, 4].Value = "CONTRACT NAME";
                    workSheet.Cells[1, 5].Value = "START DATE";
                    workSheet.Cells[1, 6].Value = "END DATE";
                    workSheet.Cells[1, 7].Value = "CUSTOMER ID";
                    workSheet.Cells[1, 8].Value = "STATUS";
                }

                int currRow = 2;

                foreach (var item in data)
                {
                    workSheet.Row(currRow).Height = 20;
                    workSheet.Cells[currRow, 1].Value = item.Id;
                    workSheet.Cells[currRow, 2].Value = item.Code;
                    workSheet.Cells[currRow, 3].Value = item.Number;
                    workSheet.Cells[currRow, 4].Value = item.Name;
                    workSheet.Cells[currRow, 5].Value = item.StartDate;
                    workSheet.Cells[currRow, 6].Value = item.EndDate;
                    workSheet.Cells[currRow, 7].Value = item.Customer != null ? item.Customer.Id : string.Empty;
                    workSheet.Cells[currRow, 8].Value = item.ContractStatus != null ? item.ContractStatus.Name : string.Empty;
                    currRow++;
                }

                workSheet.Cells.AutoFitColumns();

                return File(excel.GetAsByteArray(), "application/vnd.ms-excel", System.String.Format("{0}.xlsx", listName));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("export-sample")]
        [HasPermission(PolicyTypes.DM_HD.EXPORT_EXCEL)]
        public async Task<IActionResult> ExportSample()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string listName = "File import mẫu của danh sách hợp đồng";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 7);

                workSheet.Cells[1, 1].Value = "Mã HD";
                workSheet.Cells[1, 2].Value = "Số HD";
                workSheet.Cells[1, 3].Value = "Tên HD";
                workSheet.Cells[1, 4].Value = "Ngày bắt đầu";
                workSheet.Cells[1, 5].Value = "Ngày kết thúc";
                workSheet.Cells[1, 6].Value = "Mã KH";
                workSheet.Cells[1, 7].Value = "Trạng thái";

                workSheet.Row(2).Height = 20;
                workSheet.Cells[2, 1].Value = "LSL";
                workSheet.Cells[2, 2].Value = "86242";
                workSheet.Cells[2, 3].Value = "Electronic Frozen Computer";
                workSheet.Cells[2, 4].Value = "28/05/2024";
                workSheet.Cells[2, 5].Value = "26/06/2024";
                workSheet.Cells[2, 6].Value = "EWR";
                workSheet.Cells[2, 7].Value = "Đang thực hiện";

                workSheet.Row(3).Height = 20;
                workSheet.Cells[3, 1].Value = "NPR";
                workSheet.Cells[3, 2].Value = "36244";
                workSheet.Cells[3, 3].Value = "Electronic Soft Car";
                workSheet.Cells[3, 4].Value = "24/05/2024";
                workSheet.Cells[3, 5].Value = "16/06/2024";
                workSheet.Cells[3, 6].Value = "TUN";
                workSheet.Cells[3, 7].Value = "Hoàn thành";

                workSheet.Cells.AutoFitColumns();

                var workSheetStatus = excel.Workbook.Worksheets.Add("Trạng thái");
                workSheetStatus = ExcelExportHelper.GetStyle(workSheetStatus, 1);

                workSheetStatus.Cells[1, 1].Value = "Trạng thái";

                var data = await Mediator.Send(new RelationshipStatus_GetAllQuery(new GetAllQueryRequest()));

                int currRow = 2;

                if (data.Data != null)
                {
                    foreach (var item in data.Data)
                    {
                        workSheetStatus.Row(currRow).Height = 20;
                        workSheetStatus.Cells[currRow, 1].Value = item.Name;
                        currRow++;
                    }
                }

                workSheetStatus.Cells.AutoFitColumns();

                var workSheetCusCode = excel.Workbook.Worksheets.Add("Mã KH");
                workSheetCusCode = ExcelExportHelper.GetStyle(workSheetCusCode, 2);

                workSheetCusCode.Cells[1, 1].Value = "Mã KH";
                workSheetCusCode.Cells[1, 2].Value = "Tên KH";

                var userId = GetCurrentUser() ?? Guid.Empty;
                var dataCusCode = await Mediator.Send(new Customer_GetAllQuery(userId, new GetAllQueryRequest()));

                currRow = 2;

                if(dataCusCode.Data != null)
                {
                    foreach (var item in dataCusCode.Data)
                    {
                        workSheetCusCode.Row(currRow).Height = 20;
                        workSheetCusCode.Cells[currRow, 1].Value = item.Code;
                        workSheetCusCode.Cells[currRow, 2].Value = item.Fullname;
                        currRow++;
                    }
                }

                workSheetCusCode.Cells.AutoFitColumns();

                return File(excel.GetAsByteArray(), "application/vnd.ms-excel", System.String.Format("{0}.xlsx", listName));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
