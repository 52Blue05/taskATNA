using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.CustomerFeature.Queries;
using Sale_Saas.Application.Features.RelationshipFeature.Commands;
using Sale_Saas.Application.Features.RelationshipFeature.Queries;
using Sale_Saas.Application.Features.RelationshipFeature.RequestModels;
using Sale_Saas.Application.Features.RelationshipLevelFeature.Queries;
using Sale_Saas.Application.Features.RelationshipStatusFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Authentication;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelationshipController : BaseController
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly ILoggerService _loggerService;

        public RelationshipController(IApplicationUserService applicationUserService, ILoggerService loggerService)
        {
            _applicationUserService = applicationUserService;
            _loggerService = loggerService;
        }

        [HttpPost("filter")]
        [HasPermission(PolicyTypes.Sale_MQH.View)]
        public async Task<IActionResult> Filter([FromQuery] FilterQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Relationship_GetListQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-level/{id}")]
        [HasPermission(PolicyTypes.Sale_MQH.UPDATE_RESULT)]
        public async Task<IActionResult> GetLevel(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Relationship_GetLevelQuery(id)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-all")]
        [HasPermission(PolicyTypes.Sale_MQH.View)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Relationship_GetAllQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpPost("get-list-with-pagination")]
        public async Task<IActionResult> GetListWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userId = GetCurrentUser() ?? Guid.Empty;
                if (request.RoleType == RoleType.MYSELF.ToString())
                {
                    request.UserId = userId;
                }
                else if (userId == request.UserId)
                {
                    //Website luôn truyền UserId=user login -> cần set về null
                    request.UserId = null;
                }
                return Ok(await Mediator.Send(new Relationship_GetListWithPaginationQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-by-id/{id}")]
        [HasPermission(PolicyTypes.Sale_MQH.DETAIL)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Relationship_GetByIdQuery(id)));
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
                return Ok(await Mediator.Send(new Relationship_AddOrUpdateCommand(user, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> MobileAdd(IFormFile avatar)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Relationship_CreateAvatarCommand(avatar)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("web/add")]
        public async Task<IActionResult> WebAdd([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new Relationship_AddOrUpdate_WebCommand(user, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("web/update")]
        public async Task<IActionResult> WebUpdate([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new Relationship_AddOrUpdate_WebCommand(user, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/add")]
        public async Task<IActionResult> MobileAdd([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new Relationship_AddOrUpdate_V2Command(user, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/update")]
        public async Task<IActionResult> MobileUpdate([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new Relationship_AddOrUpdate_V2Command(user, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-list-relationship")]
        public async Task<IActionResult> GetListRelationshipMobile([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new Relationship_GetByCustomerIdQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
        [HasPermission(PolicyTypes.Sale_MQH.DELETE)]
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

                return Ok(await Mediator.Send(new Relationship_DeleteByIdCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpPut("update-result")]
        [HasPermission(PolicyTypes.Sale_MQH.UPDATE_RESULT)]
        public async Task<IActionResult> UpdateResult([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Relationship_UpdateResultCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpPut("update-level-by-id")]
        public async Task<IActionResult> UpdateLevelById([FromBody] UpdateLevelRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                request.ApplicationUserId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new Relationship_UpdateLevelByIdCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("update-status-by-id")]
        public async Task<IActionResult> UpdateStatusById([FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                request.ApplicationUserId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new Relationship_UpdateStatusByIdCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-customers")]
        public async Task<IActionResult> MobileGetListCustomers([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new RelationshipMobile_GetListCustomerQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("import")]
        [HasPermission(PolicyTypes.Sale_MQH.IMPORT_EXCEL)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                List<Relationship> listRelationships = new List<Relationship>();
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
                            var empRelationship = new Relationship();
                            empRelationship.Id = Guid.NewGuid();
                            empRelationship.DeleteFlag = false;
                            empRelationship.CreatedDate = DateTime.Now;
                            empRelationship.LastModifiedDate = DateTime.Now;
                            empRelationship.CreatedApplicationUserId = loginUserId;
                            empRelationship.LastModifiedApplicationUserId = loginUserId;
                            empRelationship.GainsId = null;
                            for (int column = 1; column <= columnCount; column++)
                            {
                                if (worksheet.Cells[row, column].Value.ToString() != null)
                                {
                                    switch (worksheet.Cells[1, column].Value.ToString())
                                    {
                                        case "Điểm mục tiêu":
                                            empRelationship.Point = int.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Điểm thực tế":
                                            empRelationship.ActualPoint = int.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Lý do cần phải nâng cấp quan hệ":
                                            empRelationship.Reason = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Vị trí làm việc":
                                            empRelationship.Position = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Tên khách hàng":
                                            empRelationship.CustomerName = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Mức độ quan hệ hiện tại":
                                            var tempCurrentRelationShipLevel = await Mediator.Send(new RelationshipLevel_GetByCodeQuery(worksheet.Cells[row, column].Value.ToString() ?? ""));
                                            if (tempCurrentRelationShipLevel.Data == null)
                                            {
                                                return Ok(Result<string>.Failure("Không tìm thấy mã của mức độ quan hệ hiện tại"));
                                            }
                                            empRelationship.CurrentRelationshipId = tempCurrentRelationShipLevel.Data.Id;
                                            break;
                                        case "Mức độ quan hệ mục tiêu":
                                            var tempTargetRelationShipLevel = await Mediator.Send(new RelationshipLevel_GetByCodeQuery(worksheet.Cells[row, column].Value.ToString() ?? ""));
                                            if (tempTargetRelationShipLevel.Data == null)
                                            {
                                                return Ok(Result<string>.Failure("Không tìm thấy mã của mức độ quan hệ mục tiêu"));
                                            }
                                            empRelationship.TargetRelationshipId = tempTargetRelationShipLevel.Data.Id;
                                            break;
                                        case "Trạng thái":
                                            var tempRelationshipStatus = await Mediator.Send(new RelationshipStatus_GetByNameQuery(worksheet.Cells[row, column].Value.ToString() ?? ""));
                                            if (tempRelationshipStatus.Data == null)
                                            {
                                                return Ok(Result<string>.Failure("Không tìm thấy trạng thái của quan hệ"));
                                            }
                                            empRelationship.RelationshipStatusId = tempRelationshipStatus.Data.Id;
                                            break;
                                        case "Mã công ty":
                                            var tempCustomer = await Mediator.Send(new Customer_GetByCodeQuery(loginUserId, worksheet.Cells[row, column].Value.ToString() ?? ""));
                                            if (tempCustomer.Data == null)
                                            {
                                                return Ok(Result<string>.Failure("Không tìm thấy mã của khách hàng"));
                                            }
                                            empRelationship.CustomerId = tempCustomer.Data.Id;
                                            break;
                                        case "Email của người chịu trách nhiệm":
                                            var userId = await _applicationUserService.GetUserIdByEmail(worksheet.Cells[row, column].Value.ToString() ?? "");
                                            if (userId.Data == Guid.Empty)
                                                return Ok(Result<string>.Failure($"Không tìm thấy nhân viên với email: {worksheet.Cells[row, column].Value.ToString()}"));
                                            empRelationship.ApplicationUserId = userId.Data;
                                            break;
                                    }
                                }
                            }
                            listRelationships.Add(empRelationship);
                        }
                    }
                }
                if (listRelationships.Count > 0)
                {
                    return Ok(await Mediator.Send(new Relationship_ImportCommand(listRelationships)));
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
        [HasPermission(PolicyTypes.Sale_MQH.EXPORT_EXCEL)]
        public async Task<IActionResult> Export([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userId = GetCurrentUser() ?? Guid.Empty;
                var response = await Mediator.Send(new Relationship_GetListWithPaginationQuery(userId, request));
                if (response.Succeeded == false || response.Data == null)
                    throw new ApplicationException("Xuất file excel thất bại!");
                var data = response.Data.Items;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string locale = GetLocale();
                string listName = locale == LocaleEnum.vi_VN.ToString() ? "Danh sách mối quan hệ" : "List relationships";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 9);

                workSheet.Cells[1, 1].Value = "TÊN CÔNG TY";
                workSheet.Cells[1, 2].Value = "TÊN KHÁCH HÀNG";
                workSheet.Cells[1, 3].Value = "VỊ TRÍ LÀM VIỆC";
                workSheet.Cells[1, 4].Value = "MỨC ĐỘ QUAN HỆ HIỆN TẠI";
                workSheet.Cells[1, 5].Value = "MỨC ĐỘ QUAN HỆ MỤC TIÊU";
                workSheet.Cells[1, 6].Value = "LÍ DO CẦN NÂNG CẤP";
                workSheet.Cells[1, 7].Value = "ĐIỂM MỤC TIÊU";
                workSheet.Cells[1, 8].Value = "ĐIỂM THỰC TẾ";
                workSheet.Cells[1, 9].Value = "TRẠNG THÁI";

                int currRow = 2;

                foreach (var item in data)
                {
                    workSheet.Row(currRow).Height = 20;
                    workSheet.Cells[currRow, 1].Value = item.Customer;
                    workSheet.Cells[currRow, 2].Value = item.CustomerName;
                    workSheet.Cells[currRow, 3].Value = item.Position;
                    workSheet.Cells[currRow, 4].Value = item.CurrentRelationshipLevel;
                    workSheet.Cells[currRow, 5].Value = item.TargetRelationshipLevel;
                    workSheet.Cells[currRow, 6].Value = item.Reason;
                    workSheet.Cells[currRow, 6].Value = item.Point;
                    workSheet.Cells[currRow, 6].Value = item.ActualPoint;
                    workSheet.Cells[currRow, 6].Value = item.RelationshipStatus.Name;

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

        [HttpGet("import-file-example")]
        public async Task<IActionResult> ImportExample()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                #region MỤC TIÊU
                var relationshipSheet = excel.Workbook.Worksheets.Add("Danh sách mối quan hệ");
                relationshipSheet = ExcelExportHelper.GetStyle(relationshipSheet, 10);

                relationshipSheet.Cells[1, 1].Value = "Điểm mục tiêu";
                relationshipSheet.Cells[1, 2].Value = "Điểm thực tế";
                relationshipSheet.Cells[1, 3].Value = "Lý do cần phải nâng cấp quan hệ";
                relationshipSheet.Cells[1, 4].Value = "Vị trí làm việc";
                relationshipSheet.Cells[1, 5].Value = "Tên khách hàng";
                relationshipSheet.Cells[1, 6].Value = "Mức độ quan hệ hiện tại";
                relationshipSheet.Cells[1, 7].Value = "Mức độ quan hệ mục tiêu";
                relationshipSheet.Cells[1, 8].Value = "Trạng thái";
                relationshipSheet.Cells[1, 9].Value = "Mã công ty";
                relationshipSheet.Cells[1, 10].Value = "Email của người chịu trách nhiệm";

                int currRow = 2;

                for (var index = 1; index <= 5; index++)
                {
                    relationshipSheet.Row(currRow).Height = 20;
                    relationshipSheet.Cells[currRow, 1].Value = $"{index}00";
                    relationshipSheet.Cells[currRow, 2].Value = $"{index}00";
                    relationshipSheet.Cells[currRow, 3].Value = $"Lí do {index}";
                    relationshipSheet.Cells[currRow, 4].Value = $"Vị trí {index}";
                    relationshipSheet.Cells[currRow, 5].Value = $"Nguyễn Văn A {index}";
                    relationshipSheet.Cells[currRow, 6].Value = "A";
                    relationshipSheet.Cells[currRow, 7].Value = "F";
                    relationshipSheet.Cells[currRow, 8].Value = "CONFIRMED";
                    relationshipSheet.Cells[currRow, 9].Value = $"COMP{index}";
                    relationshipSheet.Cells[currRow, 10].Value = $"nguyenvana{index}@gmail.com";

                    currRow++;
                }

                relationshipSheet.Cells.AutoFitColumns();
                #endregion

                #region Trạng thái
                var status = await Mediator.Send(new RelationshipStatus_GetAllQuery(new GetAllQueryRequest()));
                var statusSheet = excel.Workbook.Worksheets.Add("Danh sách trạng thái");
                statusSheet = ExcelExportHelper.GetStyle(statusSheet, 2);

                statusSheet.Cells[1, 1].Value = "Mã trạng thái";
                statusSheet.Cells[1, 2].Value = "Tên trạng thái";
                currRow = 2;

                if (status.Data != null)
                {
                    foreach (var item in status.Data)
                    {
                        statusSheet.Row(currRow).Height = 20;
                        statusSheet.Cells[currRow, 1].Value = item.Code;
                        statusSheet.Cells[currRow, 2].Value = item.Name;

                        currRow++;
                    }
                }

                statusSheet.Cells.AutoFitColumns();
                #endregion

                #region Mức độ quan hệ
                var level = await Mediator.Send(new RelationshipLevel_GetAllQuery(new GetAllQueryRequest()));
                var levelSheet = excel.Workbook.Worksheets.Add("Danh sách mức độ quan hệ");
                levelSheet = ExcelExportHelper.GetStyle(levelSheet, 4);

                levelSheet.Cells[1, 1].Value = "Mã mức độ quan hệ";
                levelSheet.Cells[1, 2].Value = "Định nghĩa";
                levelSheet.Cells[1, 3].Value = "Phần trăm phù hợp tối thiểu";
                levelSheet.Cells[1, 4].Value = "Phần trăm phù hợp tối đa";
                currRow = 2;

                if (level.Data != null)
                {
                    foreach (var item in level.Data)
                    {
                        levelSheet.Row(currRow).Height = 20;
                        levelSheet.Cells[currRow, 1].Value = item.Code;
                        levelSheet.Cells[currRow, 2].Value = item.Description;
                        levelSheet.Cells[currRow, 3].Value = item.PointFrom + "%";
                        levelSheet.Cells[currRow, 4].Value = item.PointTo + "%";

                        currRow++;
                    }
                }

                levelSheet.Cells.AutoFitColumns();
                #endregion


                return File(excel.GetAsByteArray(), "application/vnd.ms-excel", String.Format("{0}.xlsx", "Danh sách quyền lợi"));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-by-role-type")]
        public async Task<IActionResult> MobileGetListWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userId = GetCurrentUser() ?? Guid.Empty;
                if (request.RoleType == RoleType.MYSELF.ToString())
                {
                    request.UserId = userId;
                }
                var response = await Mediator.Send(new Relationship_GetListWithPaginationQuery(userId, request));
                return Ok(response);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}