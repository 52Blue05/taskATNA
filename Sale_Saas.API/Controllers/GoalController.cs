using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.GoalFeature.Commands;
using Sale_Saas.Application.Features.GoalFeature.Dto;
using Sale_Saas.Application.Features.GoalFeature.Queries;
using Sale_Saas.Application.Features.GoalFeature.Requests;
using Sale_Saas.Application.Features.GoalStatusFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Goal;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Authentication;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoalController : BaseController
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly ILoggerService _loggerService;

        public GoalController(IApplicationUserService applicationUserService, ILoggerService loggerService)
        {
            _applicationUserService = applicationUserService;
            _loggerService = loggerService;
        }

        [HttpPost("filter")]
        [HasPermission(PolicyTypes.Sale_MT.View)]
        public async Task<IActionResult> Filter([FromQuery] FilterQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Goal_GetListQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-all")]
        [HasPermission(PolicyTypes.Sale_MT.View)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Goal_GetAllQuery(userId, request)));
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
                var response = await Mediator.Send(new Goal_GetListWithPaginationQuery(userId, request));
                return Ok(response);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-by-id/{id}")]
        [HasPermission(PolicyTypes.Sale_MT.DETAIL)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Goal_GetByIdQuery(userId, id)));
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

                AddOrUpdateMediatrRequest requestData = new AddOrUpdateMediatrRequest()
                {
                    List = request,
                    TenantId = GetCurrentTenant(),
                    UserId = GetCurrentUser() ?? Guid.Empty
                };

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Goal_AddOrUpdateCommand(userId, requestData)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
        [HasPermission(PolicyTypes.Sale_MT.DELETE)]
        public async Task<IActionResult> DeleteByIds(string ids, Guid ApplicationUserId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                ApplicationUserId = GetCurrentUser() ?? Guid.Empty;

                var request = new DeleteRequest()
                {
                    Ids = ids.Split(",").ToList(),
                    ApplicationUserId = ApplicationUserId
                };

                return Ok(await Mediator.Send(new Goal_DeleteByIdCommand(ApplicationUserId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("update-status-by-id")]
        public async Task<IActionResult> UpdateStatusById([FromBody] UpdateStatusGoalRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                request.ApplicationUserId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Goal_UpdateStatusByIdCommand(request.ApplicationUserId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("update-result")]
        [HasPermission(PolicyTypes.Sale_MT.UPDATE_RESULT)]
        public async Task<IActionResult> UpdateResult([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                AddOrUpdateMediatrRequest requestData = new AddOrUpdateMediatrRequest()
                {
                    List = request,
                    TenantId = GetCurrentTenant(),
                    UserId = GetCurrentUser() ?? Guid.Empty
                };



                return Ok(await Mediator.Send(new Goal_UpdateResultCommand(requestData.UserId, requestData)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("update-benefit-id")]
        public async Task<IActionResult> UpdateBenefitId([FromBody] UpdateBenefitIdRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                request.UserId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new Goal_UpdateBenefitIdCommand(request.UserId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("import")]
        [HasPermission(PolicyTypes.Sale_MT.IMPORT_EXCEL)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                List<Goal> listGoals = new List<Goal>();
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
                            var empGoal = new Goal();
                            empGoal.Id = Guid.NewGuid();
                            empGoal.DeleteFlag = false;
                            empGoal.CreatedDate = DateTime.Now;
                            empGoal.LastModifiedDate = DateTime.Now;
                            empGoal.CreatedApplicationUserId = loginUserId;
                            empGoal.LastModifiedApplicationUserId = loginUserId;
                            empGoal.Review = null;
                            empGoal.SuggestTargetKPI = null;
                            empGoal.SuggestTargetPoint = null;
                            empGoal.UserSuggestId = loginUserId;
                            for (int column = 1; column <= columnCount; column++)
                            {
                                if (worksheet.Cells[row, column].Value.ToString() != null)
                                {
                                    switch (worksheet.Cells[1, column].Value.ToString())
                                    {
                                        case "Tiêu chí":
                                            empGoal.CriteriaName = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Mục tiêu":
                                            empGoal.TargetKPI = int.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Mục tiêu thực tế":
                                            empGoal.ActualKPI = int.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Điểm mục tiêu":
                                            empGoal.TargetPoint = int.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Thời gian bắt đầu":
                                            empGoal.StartTime = DateTime.Parse(worksheet.Cells[row, column].Value.ToString() ?? DateTime.Now.ToString());
                                            break;
                                        case "Thời gian kết thúc":
                                            empGoal.EndTime = DateTime.Parse(worksheet.Cells[row, column].Value.ToString() ?? DateTime.Now.ToString());
                                            break;
                                        case "Cách tính":
                                            empGoal.Calculate = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Điểm thực tế":
                                            empGoal.ActualPoint = int.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Trạng thái":
                                            var tempGoalStatus = await Mediator.Send(new GoalStatus_GetByNameQuery(loginUserId, worksheet.Cells[row, column].Value.ToString() ?? ""));
                                            if (tempGoalStatus.Data == null)
                                                return Ok(Result<string>.Failure("Trạng thái của mục tiêu khác trạng thái mẫu!"));
                                            empGoal.GoalStatusId = tempGoalStatus.Data.Id;
                                            break;
                                        case "Email của người đề xuất":
                                            var userId = await _applicationUserService.GetUserIdByEmail(worksheet.Cells[row, column].Value.ToString() ?? "");
                                            if (userId.Data == Guid.Empty)
                                                return Ok(Result<string>.Failure($"Không tìm thấy nhân viên với email: {worksheet.Cells[row, column].Value.ToString()}"));
                                            empGoal.UserSuggestId = Guid.Parse(userId.Data.ToString());
                                            break;
                                    }
                                }
                            }
                            empGoal.SuggestEndTime = empGoal.EndTime;
                            listGoals.Add(empGoal);
                        }
                    }
                }
                if (listGoals.Count > 0)
                {
                    var userId = GetCurrentUser() ?? Guid.Empty;
                    return Ok(await Mediator.Send(new Goal_ImportCommand(userId, listGoals)));
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
        [HasPermission(PolicyTypes.Sale_MT.EXPORT_EXCEL)]
        public async Task<IActionResult> Export([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                request.UserId = GetCurrentUser();
                var userId = GetCurrentUser() ?? Guid.Empty;
                var response = await Mediator.Send(new Goal_GetListWithPaginationQuery(userId, request));
                if (response.Succeeded == false || response.Data == null)
                    throw new ApplicationException("Xuất file excel thất bại!");
                var data = response.Data.Items;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string locale = GetLocale();
                string listName = locale == LocaleEnum.vi_VN.ToString() ? "Danh sách mục tiêu" : "List Goals";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 9);

                workSheet.Cells[1, 1].Value = "NGƯỜI ĐỀ XUẤT";
                workSheet.Cells[1, 2].Value = "TIÊU CHÍ";
                workSheet.Cells[1, 3].Value = "MỤC TIÊU";
                workSheet.Cells[1, 4].Value = "MỤC TIÊU THỰC TẾ";
                workSheet.Cells[1, 5].Value = "ĐIỂM MỤC TIÊU";
                workSheet.Cells[1, 6].Value = "THỜI GIAN THỰC HIỆN";
                workSheet.Cells[1, 7].Value = "CÁCH TÍNH";
                workSheet.Cells[1, 8].Value = "ĐIỂM THỰC TẾ";
                workSheet.Cells[1, 9].Value = "TRẠNG THÁI";

                int currRow = 2;

                foreach (var item in data)
                {
                    workSheet.Row(currRow).Height = 20;
                    workSheet.Cells[currRow, 1].Value = item.UserSuggest.FullName;
                    workSheet.Cells[currRow, 2].Value = item.CriteriaName;
                    workSheet.Cells[currRow, 3].Value = item.TargetKPI;
                    workSheet.Cells[currRow, 4].Value = item.ActualKPI;
                    workSheet.Cells[currRow, 5].Value = item.TargetPoint;
                    workSheet.Cells[currRow, 6].Value = item.StartTime.ToString("dd-mm-yyyy") + " đến " + item.EndTime.ToString("dd-mm-yyyy");
                    workSheet.Cells[currRow, 7].Value = item.Calculate;
                    workSheet.Cells[currRow, 8].Value = item.ActualPoint;
                    workSheet.Cells[currRow, 9].Value = item.GoalStatus.Name;

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
                var goalSheet = excel.Workbook.Worksheets.Add("Danh sách mục tiêu");
                goalSheet = ExcelExportHelper.GetStyle(goalSheet, 10);

                goalSheet.Cells[1, 1].Value = "Tiêu chí";
                goalSheet.Cells[1, 2].Value = "Mục tiêu";
                goalSheet.Cells[1, 3].Value = "Mục tiêu thực tế";
                goalSheet.Cells[1, 4].Value = "Điểm mục tiêu";
                goalSheet.Cells[1, 5].Value = "Thời gian bắt đầu";
                goalSheet.Cells[1, 6].Value = "Thời gian kết thúc";
                goalSheet.Cells[1, 7].Value = "Cách tính";
                goalSheet.Cells[1, 8].Value = "Điểm thực tế";
                goalSheet.Cells[1, 9].Value = "Trạng thái";
                goalSheet.Cells[1, 10].Value = "Email của người đề xuất";

                int currRow = 2;

                for (var index = 1; index <= 5; index++)
                {
                    goalSheet.Row(currRow).Height = 20;
                    goalSheet.Cells[currRow, 1].Value = $"Tiêu chí {index}";
                    goalSheet.Cells[currRow, 2].Value = $"{index}00";
                    goalSheet.Cells[currRow, 3].Value = $"{index}00";
                    goalSheet.Cells[currRow, 4].Value = $"{index}00";
                    goalSheet.Cells[currRow, 5].Value = "2024/05/05";
                    goalSheet.Cells[currRow, 6].Value = "2024/09/09";
                    goalSheet.Cells[currRow, 7].Value = $"Cách tính {index}";
                    goalSheet.Cells[currRow, 8].Value = $"{index}00";
                    goalSheet.Cells[currRow, 9].Value = "UPDATED";
                    goalSheet.Cells[currRow, 10].Value = $"nguyenvana{index}@gmail.com";

                    currRow++;
                }

                goalSheet.Cells.AutoFitColumns();
                #endregion

                #region Trạng thái
                var userId = GetCurrentUser() ?? Guid.Empty;
                var status = await Mediator.Send(new GoalStatus_GetAllQuery(userId, new GetAllQueryRequest()));
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

                return File(excel.GetAsByteArray(), "application/vnd.ms-excel", String.Format("{0}.xlsx", "Danh sách mục tiêu"));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-criteria-goal")]
        public async Task<IActionResult> GetCriteriaType()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;
                var listCriteriaType = new List<CriteriaType>();
                listCriteriaType.Add(new CriteriaType() { Code = "Criteria_Customer", Name = "Phát triển quan hệ khách hàng" });
                listCriteriaType.Add(new CriteriaType() { Code = "Criteria_2", Name = "Tiêu chí 2" });
                listCriteriaType.Add(new CriteriaType() { Code = "Criteria_3", Name = "Tiêu chí 3" });
                listCriteriaType.Add(new CriteriaType() { Code = "Criteria_Other", Name = "Khác" });
                return Ok(Result<List<CriteriaType>>.Success(listCriteriaType));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/get-by-role-type")]
        public async Task<IActionResult> MobileGetListWithPagination([FromQuery] GetListWithPaginationQueryRequest request, [FromBody] GetListGoalMobileRequest body)
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
                var response = await Mediator.Send(new GoalMobile_GetListWithPaginationQuery(userId, request, body));
                return Ok(response);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/add")]
        public async Task<IActionResult> MobileAdd([FromBody] GoalRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userLogin = GetCurrentUser() ?? Guid.Empty;
                //[{"data":{"Criteria":"Tên tiêu chí","TargetKPI":"100","TargetPoint":"200","StartTime":"02/09/2024","EndTime":"07/09/2024","ApplicationRoleId":"da5e0c7c-e6c4-4ef0-be17-5a711a7b0157","Calculate":"Cách tính","UserSuggestId":"b123c481-5c8d-4e0e-8f08-cf5190e0a92d"}}]
                List<AddOrUpdateRequest> listModel = new List<AddOrUpdateRequest>();
                listModel.Add(new AddOrUpdateRequest()
                {
                    Data = new Dictionary<string, string>
                    {
                        { "Criteria", request.Criteria },
                        { "CriteriaType", request.CriteriaType },
                        { "TargetKPI", request.TargetKPI.ToString() },
                        { "TargetPoint", request.TargetPoint.ToString() },
                        { "StartTime", request.StartTime.ToString() },
                        { "EndTime", request.EndTime.ToString() },
                        { "ActualPoint", request.ActualPoint?.ToString() },
                        { "UserSuggestId", userLogin.ToString() },
                        { "ApplicationRoleId", request.ApplicationRoleId?.ToString() },
                    },
                    Customers = request?.Customers,
                    CreatedApplicationUserId = userLogin
                });

                AddOrUpdateMediatrRequest requestData = new AddOrUpdateMediatrRequest()
                {
                    List = listModel,
                    TenantId = GetCurrentTenant(),
                    UserId = userLogin,
                };

                return Ok(await Mediator.Send(new Goal_AddOrUpdateCommand(userLogin, requestData)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("mobile/update")]
        public async Task<IActionResult> MobileUpdate([FromBody] GoalRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userLogin = GetCurrentUser() ?? Guid.Empty;
                List<AddOrUpdateRequest> listModel = new List<AddOrUpdateRequest>();
                listModel.Add(new AddOrUpdateRequest()
                {
                    Id = request.Id,
                    Data = new Dictionary<string, string>
                    {
                        { "Criteria", request.Criteria },
                        { "CriteriaType", request.CriteriaType },
                        { "TargetKPI", request.TargetKPI.ToString() },
                        { "TargetPoint", request.TargetPoint.ToString() },
                        { "StartTime", request.StartTime.ToString() },
                        { "EndTime", request.EndTime.ToString() },
                        { "ActualPoint", request.ActualPoint?.ToString() },
                        { "UserSuggestId", userLogin.ToString() },
                        { "ApplicationRoleId", request.ApplicationRoleId?.ToString() },
                    },
                    Customers = request?.Customers,
                    CreatedApplicationUserId = userLogin
                });

                AddOrUpdateMediatrRequest requestData = new AddOrUpdateMediatrRequest()
                {
                    List = listModel,
                    TenantId = GetCurrentTenant(),
                    UserId = userLogin,
                };

                return Ok(await Mediator.Send(new Goal_AddOrUpdateCommand(userLogin, requestData)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/add-v2")]
        public async Task<IActionResult> MobileAddV2([FromBody] GoalMobileRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userLogin = GetCurrentUser() ?? Guid.Empty;
                //[{"data":{"Criteria":"Tên tiêu chí","TargetKPI":"100","TargetPoint":"200","StartTime":"02/09/2024","EndTime":"07/09/2024","ApplicationRoleId":"da5e0c7c-e6c4-4ef0-be17-5a711a7b0157","Calculate":"Cách tính","UserSuggestId":"b123c481-5c8d-4e0e-8f08-cf5190e0a92d"}}]
                List<AddOrUpdateRequest> listModel = new List<AddOrUpdateRequest>();
                listModel.Add(new AddOrUpdateRequest()
                {
                    Data = new Dictionary<string, string>
                    {
                        { "CriteriaId", request.CriteriaId.ToString() },
                        { "TargetKPI", request.TargetKPI.ToString() },
                        { "Calculate", request.Calculate?.ToString() },
                        { "TargetPoint", request.TargetPoint.ToString() },
                        { "StartTime", request.StartTime.ToString() },
                        { "EndTime", request.EndTime.ToString() },
                        { "ActualPoint", request.ActualPoint?.ToString() },
                        { "UserSuggestId", userLogin.ToString() },
                        { "ApplicationRoleId", request.ApplicationRoleId?.ToString() },
                    },
                    Customers = request?.Customers,
                    CreatedApplicationUserId = userLogin
                });

                AddOrUpdateMediatrRequest requestData = new AddOrUpdateMediatrRequest()
                {
                    List = listModel,
                    TenantId = GetCurrentTenant(),
                    UserId = userLogin,
                };

                return Ok(await Mediator.Send(new GoalMobile_AddOrUpdateCommand(userLogin, requestData)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("mobile/update-v2")]
        public async Task<IActionResult> MobileUpdateV2([FromBody] GoalMobileRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userLogin = GetCurrentUser() ?? Guid.Empty;
                List<AddOrUpdateRequest> listModel = new List<AddOrUpdateRequest>();
                listModel.Add(new AddOrUpdateRequest()
                {
                    Id = request.Id,
                    Data = new Dictionary<string, string>
                    {
                        { "CriteriaId", request.CriteriaId.ToString() },
                        { "TargetKPI", request.TargetKPI.ToString() },
                        { "TargetPoint", request.TargetPoint.ToString() },
                        { "Calculate", request.Calculate?.ToString() },
                        { "StartTime", request.StartTime.ToString() },
                        { "EndTime", request.EndTime.ToString() },
                        { "ActualPoint", request.ActualPoint?.ToString() },
                        { "UserSuggestId", userLogin.ToString() },
                        { "ApplicationRoleId", request.ApplicationRoleId?.ToString() },
                    },
                    Customers = request?.Customers,
                    CreatedApplicationUserId = userLogin
                });

                AddOrUpdateMediatrRequest requestData = new AddOrUpdateMediatrRequest()
                {
                    List = listModel,
                    TenantId = GetCurrentTenant(),
                    UserId = userLogin,
                };

                return Ok(await Mediator.Send(new GoalMobile_AddOrUpdateCommand(userLogin, requestData)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("mobile/suggest-request")]
        public async Task<IActionResult> MobileUpdateStatusById([FromBody] UpdateRequestGoal request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userLogin = GetCurrentUser() ?? Guid.Empty;
                var modelRequest = new UpdateStatusGoalRequest()
                {
                    Status = GoalStatusEnum.REQUEST.ToString(),
                    Id = request.Id,
                    SuggestStartTime = request.SuggestStartTime,
                    SuggestEndTime = request.SuggestEndTime,
                    SuggestTargetKPI = request.SuggestTargetKPI,
                    SuggestTargetPoint = request.SuggestTargetPoint,
                    ApplicationUserId = userLogin
                };
                return Ok(await Mediator.Send(new Goal_UpdateStatusByIdCommand(userLogin, modelRequest)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("mobile/accept-suggest")]
        public async Task<IActionResult> MobileAcceptSuggestById([FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                request.ApplicationUserId = GetCurrentUser() ?? Guid.Empty;
                var userLogin = GetCurrentUser() ?? Guid.Empty;
                var modelRequest = new UpdateStatusGoalRequest()
                {
                    Status = "UPDATED",
                    Id = request.Id,
                    ApplicationUserId = userLogin
                };
                var result = await Mediator.Send(new Goal_UpdateStatusByIdCommand(request.ApplicationUserId, modelRequest));
                return Ok(Result<Boolean>.Success(result != null ? true : false));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("mobile/approve")]
        public async Task<IActionResult> MobileApproveById([FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                request.ApplicationUserId = GetCurrentUser() ?? Guid.Empty;
                var userLogin = GetCurrentUser() ?? Guid.Empty;
                var modelRequest = new UpdateStatusGoalRequest()
                {
                    Status = "PROCESSING",
                    Id = request.Id,
                    ApplicationUserId = userLogin
                };
                var result = await Mediator.Send(new Goal_UpdateStatusByIdCommand(request.ApplicationUserId, modelRequest));
                return Ok(Result<Boolean>.Success(result != null ? true : false));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("mobile/delete-by-ids/{ids}")]
        [HasPermission(PolicyTypes.Sale_MT.DELETE)]
        public async Task<IActionResult> MobileDeleteByIds(string ids)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userLogin = GetCurrentUser() ?? Guid.Empty;

                var request = new DeleteRequest()
                {
                    Ids = ids.Split(",").ToList(),
                    ApplicationUserId = userLogin
                };
                return Ok(await Mediator.Send(new Goal_DeleteByIdCommand(userLogin, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-by-id/{id}")]
        [HasPermission(PolicyTypes.Sale_MT.DETAIL)]
        public async Task<IActionResult> MobileGetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new GoalMobile_GetByIdQuery(userId, id)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-by-benefit/{id}")]
        public async Task<IActionResult> MobileGetByBenefit(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;
                GetListWithPaginationQueryRequest request = new GetListWithPaginationQueryRequest()
                {
                    BenefitId = id
                };
                var response = await Mediator.Send(new Goal_GetListWithPaginationQuery(userId, request));
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
