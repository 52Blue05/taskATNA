using Microsoft.AspNetCore.Mvc;
using Minio.DataModel;
using OfficeOpenXml;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.ProjectFeature.Commands;
using Sale_Saas.Application.Features.ProjectFeature.Queries;
using Sale_Saas.Application.Features.ProjectStatusFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Project;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Authentication;
using Sale_Saas.Infrastructure.Services.TenantService;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : BaseController
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly ILoggerService _loggerService;

        public ProjectController(IApplicationUserService applicationUserService, ILoggerService loggerService)
        {
            _applicationUserService = applicationUserService;
            _loggerService = loggerService;
        }

        [HttpPost("filter")]
        [HasPermission(PolicyTypes.DM_DA.VIEW)]
        public async Task<IActionResult> Filter([FromQuery] FilterQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Project_GetListQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-all")]
        [HasPermission(PolicyTypes.DM_DA.VIEW)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Project_GetAllQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpPost("get-list-with-pagination")]
        //[HasPermission(PolicyTypes.DM_DA.DETAIL)]
        [HasPermission(PolicyTypes.DM_DA.VIEW)]
        public async Task<IActionResult> GetListWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentTenant = GetCurrentTenant();

                return Ok(await Mediator.Send(new Project_GetListWithPaginationQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-by-id/{id}")]
        //[HasPermission(PolicyTypes.DM_DA.DETAIL)]
        [HasPermission(PolicyTypes.DM_DA.VIEW)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Project_GetByIdQuery(id)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("add-or-update")]
        [HasPermission(PolicyTypes.DM_DA.CREATE)]
        public async Task<IActionResult> AddOrUpdate([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Project_AddOrUpdateCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
        [HasPermission(PolicyTypes.DM_DA.DELETE)]
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

                return Ok(await Mediator.Send(new Project_DeleteByIdCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("update")]
        [HasPermission(PolicyTypes.DM_DA.UPDATE)]
        public async Task<IActionResult> Update([FromBody] List<ProjectUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Project_UpdateCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("update-result")]
        [HasPermission(PolicyTypes.DM_DA.UPDATE_RESULT)]
        public async Task<IActionResult> UpdateResult([FromBody] List<ProjectUpdateResultRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Project_UpdateResultCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("import")]
        [HasPermission(PolicyTypes.DM_DA.IMPORT_EXCEL)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                List<Project> listProjects = new List<Project>();
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
                            var empproject = new Project();
                            empproject.Id = Guid.NewGuid();
                            empproject.DeleteFlag = false;
                            empproject.CreatedDate = DateTime.Now;
                            empproject.LastModifiedDate = DateTime.Now;
                            empproject.CreatedApplicationUserId = loginUserId;
                            empproject.LastModifiedApplicationUserId = loginUserId;
                            for (int column = 1; column <= columnCount; column++)
                            {
                                if (worksheet.Cells[row, column].Value.ToString() != null)
                                {
                                    switch (worksheet.Cells[1, column].Value.ToString())
                                    {
                                        case "Mã dự án":
                                            empproject.Code = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Tên dự án":
                                            empproject.Name = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Kết quả":
                                            empproject.Result = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Loại dự án":
                                            empproject.Type = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Điểm đạt":
                                            empproject.Point = int.Parse(worksheet.Cells[row, column].Value.ToString());
                                            break;
                                        case "Ghi chú":
                                            empproject.Note = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Mảng kinh doanh":
                                            empproject.Service = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Phụ trách":
                                            string tempUserId = (await Mediator.Send(await _applicationUserService.GetUserIdByCode(worksheet.Cells[row, column].Value.ToString()))).ToString();
                                            if(tempUserId == null)
                                                return Ok(Result<string>.Failure("Không thể tìm thấy phụ trách"));
                                            empproject.ApplicationUserId = Guid.Parse(tempUserId);
                                            break;
                                        case "Trạng thái":
                                            var tempProjectStatus = await Mediator.Send(new ProjectStatus_GetByNameQuery(worksheet.Cells[row, column].Value.ToString()));
                                            if (tempProjectStatus.Data == null)
                                                return Ok(Result<string>.Failure("Trạng thái của dự án khác trạng thái mẫu!"));
                                            empproject.ProjectStatusId = tempProjectStatus.Data.Id;
                                            break;                                        
                                    }
                                }
                            }
                            listProjects.Add(empproject);
                        }
                    }
                }
                if (listProjects.Count > 0)
                {
                    return Ok(await Mediator.Send(new Project_ImportCommand(listProjects)));
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
        [HasPermission(PolicyTypes.DM_DA.EXPORT_EXCEL)]
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
                var response = await Mediator.Send(new Project_GetListWithPaginationQuery(request));
                if (response.Succeeded == false || response.Data == null)
                    throw new ApplicationException("Xuất file excel thất bại!");
                var data = response.Data.Items;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string listName = locale == LocaleEnum.vi_VN.ToString() ? "Danh sách dự án" : "List projects";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 10);

                workSheet.Cells[1, 1].Value = "ID";
                workSheet.Cells[1, 2].Value = "MÃ DỰ ÁN";
                workSheet.Cells[1, 3].Value = "TÊN DỰ ÁN";
                workSheet.Cells[1, 4].Value = "KẾT QUẢ";
                workSheet.Cells[1, 5].Value = "LOẠI DỰ ÁN";
                workSheet.Cells[1, 6].Value = "ĐIỂM ĐẠT";
                workSheet.Cells[1, 7].Value = "GHI CHÚ";
                workSheet.Cells[1, 8].Value = "MẢNG KINH DOANH";
                workSheet.Cells[1, 9].Value = "PHỤ TRÁCH";
                workSheet.Cells[1, 10].Value = "TRẠNG THÁI";

                if (locale == LocaleEnum.en_US.ToString())
                {
                    workSheet.Cells[1, 2].Value = "PROJECT ID";
                    workSheet.Cells[1, 3].Value = "PROJECT NAME";
                    workSheet.Cells[1, 4].Value = "RESULT";
                    workSheet.Cells[1, 5].Value = "PROJECT TYPE";
                    workSheet.Cells[1, 6].Value = "POINT";
                    workSheet.Cells[1, 7].Value = "NOTE";
                    workSheet.Cells[1, 8].Value = "BUSINESS DIVISION";
                    workSheet.Cells[1, 9].Value = "RESPONSIBLE FOR";
                    workSheet.Cells[1, 10].Value = "STATUS";
                }

                int currRow = 2;

                foreach (var item in data)
                {
                    workSheet.Row(currRow).Height = 20;
                    workSheet.Cells[currRow, 1].Value = item.Id;
                    workSheet.Cells[currRow, 2].Value = item.Code;
                    workSheet.Cells[currRow, 3].Value = item.Name;
                    workSheet.Cells[currRow, 4].Value = item.Result;
                    workSheet.Cells[currRow, 5].Value = item.Type;
                    workSheet.Cells[currRow, 6].Value = item.Point;
                    workSheet.Cells[currRow, 7].Value = item.Note;
                    workSheet.Cells[currRow, 8].Value = item.Service;
                    workSheet.Cells[currRow, 9].Value = item.ApplicationUser;
                    workSheet.Cells[currRow, 10].Value = item.ProjectStatus.Name;

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
        [HasPermission(PolicyTypes.DM_DA.EXPORT_EXCEL)]
        public async Task<IActionResult> ExportSample()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string listName = "File import mẫu của danh sách dự án";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 9);

                workSheet.Cells[1, 1].Value = "Mã dự án";
                workSheet.Cells[1, 2].Value = "Tên dự án";
                workSheet.Cells[1, 3].Value = "Kết quả";
                workSheet.Cells[1, 4].Value = "Loại dự án";
                workSheet.Cells[1, 5].Value = "Điểm đạt";
                workSheet.Cells[1, 6].Value = "Ghi chú";
                workSheet.Cells[1, 7].Value = "Mảng kinh doanh";
                workSheet.Cells[1, 8].Value = "Mã của người phụ trách";
                workSheet.Cells[1, 9].Value = "Trạng thái";

                workSheet.Row(2).Height = 20;
                workSheet.Cells[2, 1].Value = "144305009730";
                workSheet.Cells[2, 2].Value = "Catherine Payne";
                workSheet.Cells[2, 3].Value = "If the plan doesn’t";
                workSheet.Cells[2, 4].Value = "Success consists of going";
                workSheet.Cells[2, 5].Value = "184";
                workSheet.Cells[2, 6].Value = "Monitored servers include";
                workSheet.Cells[2, 7].Value = "Baggage & Travel Equipment";
                workSheet.Cells[2, 8].Value = "090624-RBVOUY";
                workSheet.Cells[2, 9].Value = "Đang thực hiện";

                workSheet.Row(3).Height = 20;
                workSheet.Cells[3, 1].Value = "34829257";
                workSheet.Cells[3, 2].Value = "Cynthia Lopez";
                workSheet.Cells[3, 3].Value = "Navicat Data Modeler";
                workSheet.Cells[3, 4].Value = "Import Wizard allows";
                workSheet.Cells[3, 5].Value = "131";
                workSheet.Cells[3, 6].Value = "It provides strong authentication";
                workSheet.Cells[3, 7].Value = "Musical Instrument";
                workSheet.Cells[3, 8].Value = "230624-ZBMEBW";
                workSheet.Cells[3, 9].Value = "Hoàn thành";

                workSheet.Cells.AutoFitColumns();

                var workSheetStatus = excel.Workbook.Worksheets.Add("Trạng thái");
                workSheetStatus = ExcelExportHelper.GetStyle(workSheetStatus, 1);

                workSheetStatus.Cells[1, 1].Value = "Trạng thái";

                var dataStatus = await Mediator.Send(new ProjectStatus_GetAllQuery(new GetAllQueryRequest()));

                int currRow = 2;
                foreach (var item in dataStatus.Data)
                {
                    workSheetStatus.Row(currRow).Height = 20;
                    workSheetStatus.Cells[currRow, 1].Value = item.Name;

                    currRow++;
                }

                workSheetStatus.Cells.AutoFitColumns();

                var workSheetCode = excel.Workbook.Worksheets.Add("Mã của người phụ trách");
                workSheetCode = ExcelExportHelper.GetStyle(workSheetCode, 3);

                workSheetCode.Cells[1, 1].Value = "Họ và Tên";
                workSheetCode.Cells[1, 2].Value = "Email";
                workSheetCode.Cells[1, 3].Value = "Mã";

                var dataCode = await _applicationUserService.GetAllQueryAsync(new GetAllQueryRequest());

                currRow = 2;
                foreach (var item in dataCode.Data)
                {
                    workSheetCode.Row(currRow).Height = 20;
                    workSheetCode.Cells[currRow, 1].Value = item.FullName;
                    workSheetCode.Cells[currRow, 2].Value = item.Email;
                    workSheetCode.Cells[currRow, 3].Value = item.Code;
                    currRow++;
                }

                workSheetCode.Cells.AutoFitColumns();

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
