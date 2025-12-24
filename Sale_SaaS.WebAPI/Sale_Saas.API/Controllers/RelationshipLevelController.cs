using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.RelationshipLevelFeature.Commands;
using Sale_Saas.Application.Features.RelationshipLevelFeature.Queries;
using Sale_Saas.Application.Features.SupplierFeature.Commands;
using Sale_Saas.Application.Features.SupplierFeature.Queries;
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
    public class RelationshipLevelController : BaseController
    {

        private readonly ILoggerService _loggerService;

        public RelationshipLevelController(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpPost("filter")]
        [HasPermission(PolicyTypes.DM_MDQH.SEARCH)]
        public async Task<IActionResult> Filter([FromQuery] FilterQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new RelationshipLevel_GetListQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-all")]
        [HasPermission(PolicyTypes.DM_MDQH.SEARCH)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new RelationshipLevel_GetAllQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpPost("get-list-with-pagination")]
        [HasPermission(PolicyTypes.DM_MDQH.VIEW)]
        public async Task<IActionResult> GetListWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new RelationshipLevel_GetListWithPaginationQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-by-id/{id}")]
        [HasPermission(PolicyTypes.DM_MDQH.VIEW)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new RelationshipLevel_GetByIdQuery(id)));
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

                return Ok(await Mediator.Send(new RelationshipLevel_AddOrUpdateCommand(userId,request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
        [HasPermission(PolicyTypes.DM_MDQH.DELETE)]
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

                return Ok(await Mediator.Send(new RelationshipLevel_DeleteByIdCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("import")]
        [HasPermission(PolicyTypes.DM_MDQH.IMPORT_EXCEL)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                List<RelationshipLevel> listrelationshiplevels = new List<RelationshipLevel>();
                var loginUserId = GetCurrentUser();
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
                            var emprelationshiplevel = new RelationshipLevel();
                            emprelationshiplevel.Id = Guid.NewGuid();
                            emprelationshiplevel.DeleteFlag = false;
                            emprelationshiplevel.CreatedDate = DateTime.Now;
                            emprelationshiplevel.LastModifiedDate = DateTime.Now;
                            emprelationshiplevel.CreatedApplicationUserId = loginUserId;
                            emprelationshiplevel.LastModifiedApplicationUserId = loginUserId;
                            emprelationshiplevel.Review = string.Empty;
                            for (int column = 1; column <= columnCount; column++)
                            {
                                if (worksheet.Cells[row, column].Value.ToString() != null)
                                {
                                    switch (worksheet.Cells[1, column].Value.ToString())
                                    {
                                        case "Mức":
                                            emprelationshiplevel.Code = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Định nghĩa":
                                            emprelationshiplevel.Description = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        //case "Review":
                                        //    emprelationshiplevel.Review = worksheet.Cells[row, column].Value.ToString();
                                        //    break;
                                        case "Phần trăm phù hợp tối thiểu":
                                            emprelationshiplevel.PointFrom = int.Parse(worksheet.Cells[row, column].Value.ToString());
                                            break;
                                        case "Phần trăm phù hợp tối đa":
                                            emprelationshiplevel.PointTo = int.Parse(worksheet.Cells[row, column].Value.ToString());
                                            break;
                                    }
                                }
                            }
                            listrelationshiplevels.Add(emprelationshiplevel);
                        }
                    }
                }
                if (listrelationshiplevels.Count > 0)
                {
                    return Ok(await Mediator.Send(new RelationshipLevel_ImportCommand(listrelationshiplevels)));
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
        [HasPermission(PolicyTypes.DM_MDQH.EXPORT_EXCEL)]
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
                var response = await Mediator.Send(new RelationshipLevel_GetListWithPaginationQuery(request));
                if (response.Succeeded == false || response.Data == null)
                    throw new ApplicationException("Xuất file excel thất bại!");
                var data = response.Data.Items;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string listName = locale == LocaleEnum.vi_VN.ToString() ? "Danh sách mức độ quan hệ" : "List relationship levels";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 5);

                workSheet.Cells[1, 1].Value = "ID";
                workSheet.Cells[1, 2].Value = "MỨC";
                workSheet.Cells[1, 3].Value = "ĐỊNH NGHĨA";
                workSheet.Cells[1, 4].Value = "PHẦN TRĂM PHÙ HỢP TỐI THIỂU";
                workSheet.Cells[1, 5].Value = "PHẦN TRĂM PHÙ HỢP TỐI ĐA";

                if (locale == LocaleEnum.en_US.ToString())
                {
                    workSheet.Cells[1, 2].Value = "LEVEL";
                    workSheet.Cells[1, 3].Value = "DESCRIPTION";
                    workSheet.Cells[1, 4].Value = "MINIMUM MATCH PERCENTAGE";
                    workSheet.Cells[1, 5].Value = "MAXIMUM MATCH PERCENTAGE";
                }

                int currRow = 2;

                foreach (var item in data)
                {
                    workSheet.Row(currRow).Height = 20;
                    workSheet.Cells[currRow, 1].Value = item.Id;
                    workSheet.Cells[currRow, 2].Value = item.Code;
                    workSheet.Cells[currRow, 3].Value = item.Description;
                    workSheet.Cells[currRow, 4].Value = item.PointFrom;
                    workSheet.Cells[currRow, 5].Value = item.PointTo;

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
        [HasPermission(PolicyTypes.DM_MDQH.EXPORT_EXCEL)]
        public async Task<IActionResult> ExportSamplle([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string listName = "File import mẫu của mức độ quan hệ";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 4);

                workSheet.Cells[1, 1].Value = "Mức";
                workSheet.Cells[1, 2].Value = "Định nghĩa";
                workSheet.Cells[1, 3].Value = "Phần trăm phù hợp tối thiểu";
                workSheet.Cells[1, 4].Value = "Phần trăm phù hợp tối đa";

                workSheet.Row(2).Height = 20;
                workSheet.Cells[2, 1].Value = "A";
                workSheet.Cells[2, 2].Value = "Mới có thông tin liên hệ và chưa có sự chủ động nhờ vã nào";
                workSheet.Cells[2, 3].Value = "0";
                workSheet.Cells[2, 4].Value = "30";

                workSheet.Row(3).Height = 20;
                workSheet.Cells[3, 1].Value = "B";
                workSheet.Cells[3, 2].Value = "Là mức A và đã có 5 lần chia sẽ và nhờ vã";
                workSheet.Cells[3, 3].Value = "31";
                workSheet.Cells[3, 4].Value = "40";

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
