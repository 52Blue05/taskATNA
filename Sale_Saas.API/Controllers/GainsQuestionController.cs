using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.GainsQuestionFeature.Commands;
using Sale_Saas.Application.Features.GainsQuestionFeature.Queries;
using Sale_Saas.Application.Features.GainsQuestionFeature.Requests;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Authentication;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GainsQuestionController : BaseController
    {

        private readonly ILoggerService _loggerService;

        public GainsQuestionController(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpPost("filter")]
        [HasPermission(PolicyTypes.DM_GAINS.VIEW)]
        public async Task<IActionResult> Filter([FromQuery] FilterQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                /*var user = GetCurrentUser() ?? Guid.Empty;
                var access = await _permissionService.HasPermission(user, MenuEnum.dm_g.ToString(), PermissionType.permission.ViewProfile);
                if (access == false) throw new ApplicationException("Tài khoản không đủ quyền truy cập");*/

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new GainsQuestion_GetListQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-all")]
        [HasPermission(PolicyTypes.DM_GAINS.VIEW)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new GainsQuestion_GetAllQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpPost("get-list-with-pagination")]
        [HasPermission(PolicyTypes.DM_GAINS.VIEW)]
        public async Task<IActionResult> GetListWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new GainsQuestion_GetListWithPaginationQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-list-with-pagination")]
        [HasPermission(PolicyTypes.DM_GAINS.VIEW)]
        public async Task<IActionResult> MobileGetListWithPagination([FromQuery] GainsQuestionGetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new GainsQuestionMobile_GetListWithPaginationQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-by-id/{id}")]
        [HasPermission(PolicyTypes.DM_GAINS.VIEW)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new GainsQuestion_GetByIdQuery(userId, id)));
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

                return Ok(await Mediator.Send(new GainsQuestion_AddOrUpdateCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
        [HasPermission(PolicyTypes.DM_GAINS.DELETE)]
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

                return Ok(await Mediator.Send(new GainsQuestion_DeleteByIdCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("import")]
        [HasPermission(PolicyTypes.DM_GAINS.IMPORT_EXCEL)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                List<GainsQuestion> listGainsQuestion = new List<GainsQuestion>();
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
                            var empgainsquestion = new GainsQuestion();
                            empgainsquestion.Id = Guid.NewGuid();
                            empgainsquestion.DeleteFlag = false;
                            empgainsquestion.CreatedDate = DateTime.Now;
                            empgainsquestion.LastModifiedDate = DateTime.Now;
                            empgainsquestion.CreatedApplicationUserId = loginUserId;
                            empgainsquestion.LastModifiedApplicationUserId = loginUserId;
                            empgainsquestion.Description = string.Empty;
                            for (int column = 1; column <= columnCount; column++)
                            {
                                if (worksheet.Cells[row, column].Value.ToString() != null)
                                {
                                    switch (worksheet.Cells[1, column].Value.ToString())
                                    {
                                        case "Stt":
                                            empgainsquestion.Code = int.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Câu hỏi":
                                            empgainsquestion.Content = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                    }
                                }
                            }
                            listGainsQuestion.Add(empgainsquestion);
                        }
                    }
                }
                if (listGainsQuestion.Count > 0)
                {
                    var userId = GetCurrentUser() ?? Guid.Empty;
                    return Ok(await Mediator.Send(new GainsQuestion_ImportCommand(userId, listGainsQuestion)));
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
        [HasPermission(PolicyTypes.DM_GAINS.EXPORT_EXCEL)]
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
                var response = await Mediator.Send(new GainsQuestion_GetListWithPaginationQuery(userId, request));
                if (response.Succeeded == false || response.Data == null)
                    throw new ApplicationException("Xuất file excel thất bại!");
                var data = response.Data.Items;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string listName = locale == LocaleEnum.vi_VN.ToString() ? "Danh sách câu hỏi bằng GAINS" : "List question by GAINS";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 3);

                workSheet.Cells[1, 1].Value = "ID";
                workSheet.Cells[1, 2].Value = "STT";
                workSheet.Cells[1, 3].Value = "CÂU HỎI";

                if (locale == LocaleEnum.en_US.ToString())
                {
                    workSheet.Cells[1, 2].Value = "SUPPLIER CODE";
                    workSheet.Cells[1, 3].Value = "CUSTOMER FULL NAME";
                }

                int currRow = 2;

                foreach (var item in data)
                {
                    workSheet.Row(currRow).Height = 20;
                    workSheet.Cells[currRow, 1].Value = item.Id;
                    workSheet.Cells[currRow, 2].Value = item.Content;

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
        [HasPermission(PolicyTypes.DM_GAINS.EXPORT_EXCEL)]
        public async Task<IActionResult> ExportSample([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string listName = "File import mẫu của câu hỏi bằng GAINS";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 2);

                workSheet.Cells[1, 1].Value = "Stt";
                workSheet.Cells[1, 2].Value = "Câu hỏi";

                workSheet.Row(2).Height = 20;
                workSheet.Cells[2, 1].Value = "15";
                workSheet.Cells[2, 2].Value = "Bạn biết rõ về thông tin cá nhân của đối tác? (tên, quê, ngày sinh, nơi ở, sở thích, kỹ năng)";

                workSheet.Row(3).Height = 20;
                workSheet.Cells[3, 1].Value = "8";
                workSheet.Cells[3, 2].Value = "Bạn biết rõ về thông tin kinh nghiệm của đối tác? (học ở những trường nào? đã từng làm ở những vị trí nào, của công ty nào? điểm mạnh hoặc sở trường)";

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
