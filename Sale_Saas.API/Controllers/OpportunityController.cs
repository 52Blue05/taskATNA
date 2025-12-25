using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.OpportunityFeature.Commands;
using Sale_Saas.Application.Features.OpportunityFeature.Dto;
using Sale_Saas.Application.Features.OpportunityFeature.Queries;
using Sale_Saas.Application.Features.OpportunityFeature.Requests;
using Sale_Saas.Application.Features.OpportunityStatusFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Authentication;
using System.Reflection;
using static Sale_Saas.Application.Features.OpportunityStatusFeature.Requests;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpportunityController : BaseController
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly ILoggerService _loggerService;

        public OpportunityController(IApplicationUserService applicationUserService, ILoggerService loggerService)
        {
            _applicationUserService = applicationUserService;
            _loggerService = loggerService;
        }
        [HttpPost("filter")]
        [HasPermission(PolicyTypes.Sale_CH.View)]
        public async Task<IActionResult> Filter([FromQuery] FilterQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Opportunity_GetListQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-all")]
        [HasPermission(PolicyTypes.Sale_CH.View)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Opportunity_GetAllQuery(request)));
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

                return Ok(await Mediator.Send(new Opportunity_GetListWithPaginationQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-by-id/{id}")]
        //[HasPermission(PolicyTypes.Sale_CH.DETAIL)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Opportunity_GetByIdQuery(id)));
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
                return Ok(await Mediator.Send(new Opportunity_AddOrUpdateCommand(user, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("get-by-role-type")]
        public async Task<IActionResult> GetListWithPaginationV2([FromQuery] OpportunityGetListWithPaginationRequest request)
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

                return Ok(await Mediator.Send(new Opportunity_GetListWithPaginationV2Query(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("add-v2")]
        public async Task<IActionResult> AddV2([FromBody] CreateOrUpdateOpportunityV2Request request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Opportunity_AddV2Command(user, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("update-v2")]
        public async Task<IActionResult> UpdateV2([FromBody] CreateOrUpdateOpportunityV2Request request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Opportunity_UpdateV2Command(user, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
        [HasPermission(PolicyTypes.Sale_CH.DELETE)]
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

                return Ok(await Mediator.Send(new Opportunity_DeleteByIdCommand(request)));
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
                return Ok(await Mediator.Send(new Opportunity_UpdateStatusByIdCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("assign-user")]
        [HasPermission(PolicyTypes.Sale_CH.CH_GANCOHOI)]
        public async Task<IActionResult> AssignUser([FromBody] AssignUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Opportunity_AssignUserCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("import")]
        [HasPermission(PolicyTypes.Sale_CH.IMPORT_EXCEL)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                List<Opportunity> listOpportunity = new List<Opportunity>();
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
                            var empOpportunity = new Opportunity();
                            empOpportunity.Id = Guid.NewGuid();
                            empOpportunity.DeleteFlag = false;
                            empOpportunity.CreatedDate = DateTime.Now;
                            empOpportunity.LastModifiedDate = DateTime.Now;
                            empOpportunity.CreatedApplicationUserId = loginUserId;
                            empOpportunity.LastModifiedApplicationUserId = loginUserId;
                            empOpportunity.WinningOppotunity = null;
                            for (int column = 1; column <= columnCount; column++)
                            {
                                if (worksheet.Cells[row, column].Value.ToString() != null)
                                {
                                    switch (worksheet.Cells[1, column].Value.ToString())
                                    {
                                        case "Email của người nhận cơ hội":
                                            var userId = await _applicationUserService.GetUserIdByEmail(worksheet.Cells[row, column].Value.ToString());
                                            if (userId == null)
                                                return Ok(Result<string>.Failure($"Không tìm thấy nhân viên với email: {worksheet.Cells[row, column].Value.ToString()}"));
                                            empOpportunity.ApplicationUserId = userId.Data;
                                            break;
                                        case "Ngân sách":
                                            empOpportunity.Budget = int.Parse(worksheet.Cells[row, column].Value.ToString());
                                            break;
                                        case "Tên khách hàng":
                                            empOpportunity.CustomerName = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Trạng thái":
                                            var tempBenefitStatus = await Mediator.Send(new OpportunityStatus_GetByNameQuery(worksheet.Cells[row, column].Value.ToString()));
                                            if (tempBenefitStatus.Data == null)
                                                return Ok(Result<string>.Failure("Trạng thái của cơ hội khác trạng thái mẫu"));
                                            empOpportunity.OpportunityStatusId = tempBenefitStatus.Data.Id;
                                            break;
                                        case "Nhu cầu khách hàng":
                                            empOpportunity.Need = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Người phụ trách kỹ thuật":
                                            empOpportunity.TechnicalLead = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Thời điểm dự kiến cơ hội diễn ra":
                                            empOpportunity.EstimatedTime = DateTime.Parse(worksheet.Cells[row, column].Value.ToString());
                                            break;
                                        case "Giá vốn dự kiến":
                                            empOpportunity.EstimatedMoney = int.Parse(worksheet.Cells[row, column].Value.ToString());
                                            break;
                                        case "Hoa hồng cho người tư vấn":
                                            empOpportunity.CommissionMoney = int.Parse(worksheet.Cells[row, column].Value.ToString());
                                            break;
                                        case "Đối thủ 1":
                                            empOpportunity.Opponent1 = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        //case "Opponent1Attribute":
                                        //    empOpportunity.Opponent1Attribute = worksheet.Cells[row, column].Value.ToString();
                                        //break;
                                        case "Đối thủ 2":
                                            empOpportunity.Opponent2 = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        //case "Opponent2Attribute":
                                        //    empOpportunity.Opponent2Attribute = worksheet.Cells[row, column].Value.ToString();
                                        //    break;
                                        case "Chiến lược của công ty":
                                            empOpportunity.Strategy = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Lần tương tác gần nhất giữa AT&A và khách hàng":
                                            empOpportunity.LastTimeInteract = DateTime.Parse(worksheet.Cells[row, column].Value.ToString());
                                            break;
                                        case "Người quyết định":
                                            empOpportunity.Accountable = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Người thụ hưởng":
                                            empOpportunity.Beneficiary = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        //case "Lí do":
                                        //    empOpportunity.Reason = worksheet.Cells[row, column].Value.ToString();
                                        //    break;
                                        case "Đánh giá khả năng thắng":
                                            empOpportunity.WinningOppotunity = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                    }
                                }
                            }
                            listOpportunity.Add(empOpportunity);
                        }
                    }
                }
                if (listOpportunity.Count > 0)
                {
                    return Ok(await Mediator.Send(new Opportunity_ImportCommand(listOpportunity)));
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
        [HasPermission(PolicyTypes.Sale_CH.EXPORT_EXCEL)]
        public async Task<IActionResult> Export([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                request.UserId = GetCurrentUser();
                var response = await Mediator.Send(new Opportunity_GetListWithPaginationQuery(request));
                if (response.Succeeded == false || response.Data == null)
                    throw new ApplicationException("Xuất file excel thất bại!");
                var data = response.Data.Items;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string locale = GetLocale();
                string listName = locale == LocaleEnum.vi_VN.ToString() ? "Danh sách cơ hội" : "List Opportunitis";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 15);

                workSheet.Cells[1, 1].Value = "KHÁCH HÀNG";
                workSheet.Cells[1, 2].Value = "NHU CẦU KHÁCH HÀNG";
                workSheet.Cells[1, 3].Value = "NGƯỜI QUYẾT ĐỊNH";
                workSheet.Cells[1, 4].Value = "NGÂN SÁCH";
                workSheet.Cells[1, 5].Value = "THỜI ĐIỂM DỰ KIẾN";
                workSheet.Cells[1, 6].Value = "PHỤ TRÁCH KỸ THUẬT";
                workSheet.Cells[1, 7].Value = "NGƯỜI THỤ HƯỞNG";
                workSheet.Cells[1, 8].Value = "GIÁ VỐN DỮ KIẾN";
                workSheet.Cells[1, 9].Value = "HOA HỒNG NGƯỜI TƯ VẤN";
                workSheet.Cells[1, 10].Value = "ĐỐI THỦ 1";
                workSheet.Cells[1, 11].Value = "ĐỐI THỦ 2";
                workSheet.Cells[1, 12].Value = "CHIẾN LƯỢC AT&A";
                workSheet.Cells[1, 13].Value = "LẦN TƯƠNG TÁC GẦN NHẤT";
                workSheet.Cells[1, 14].Value = "ĐÁNH GIÁ KHẢ NĂNG THẮNG";
                workSheet.Cells[1, 15].Value = "TRẠNG THÁI";

                if (locale != LocaleEnum.vi_VN.ToString())
                {
                    workSheet.Cells[1, 1].Value = "CUSTOMERs";
                    workSheet.Cells[1, 2].Value = "CUSTOMER NEED";
                    workSheet.Cells[1, 3].Value = "DECISION-MAKER";
                    workSheet.Cells[1, 4].Value = "BUDGET";
                    workSheet.Cells[1, 5].Value = "EXPECTED OPPORTUNITY OCCURRENCE TIME";
                    workSheet.Cells[1, 6].Value = "TECHNICAL LEAD";
                    workSheet.Cells[1, 7].Value = "BENEFICIARIES";
                    workSheet.Cells[1, 8].Value = "ESTIMATED MONEY";
                    workSheet.Cells[1, 9].Value = "COMMISSION MONEY";
                    workSheet.Cells[1, 10].Value = "OPPONENT 1";
                    workSheet.Cells[1, 11].Value = "OPPONENT 2";
                    workSheet.Cells[1, 12].Value = "STRATEGY";
                    workSheet.Cells[1, 13].Value = "LAST TIME INTERACT";
                    workSheet.Cells[1, 14].Value = "WINNING OPPOTUNITY";
                    workSheet.Cells[1, 15].Value = "STATUS";
                }

                int currRow = 2;

                foreach (var item in data)
                {
                    workSheet.Row(currRow).Height = 20;
                    workSheet.Cells[currRow, 1].Value = item.CustomerName;
                    workSheet.Cells[currRow, 2].Value = item.Need;
                    workSheet.Cells[currRow, 3].Value = item.Accountable;
                    workSheet.Cells[currRow, 4].Value = item.Budget;
                    workSheet.Cells[currRow, 5].Value = item.EstimatedTime.ToString("dd-mm-yyyy");
                    workSheet.Cells[currRow, 6].Value = item.TechnicalLead;
                    workSheet.Cells[currRow, 7].Value = item.Beneficiary;
                    workSheet.Cells[currRow, 8].Value = item.EstimatedMoney;
                    workSheet.Cells[currRow, 9].Value = item.CommissionMoney;
                    workSheet.Cells[currRow, 10].Value = item.Opponent1;
                    workSheet.Cells[currRow, 11].Value = item.Opponent2;
                    workSheet.Cells[currRow, 12].Value = item.Strategy;
                    workSheet.Cells[currRow, 13].Value = item.LastTimeInteract;
                    workSheet.Cells[currRow, 14].Value = item.WinningOppotunity;
                    workSheet.Cells[currRow, 15].Value = item.OpportunityStatus.Name;

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
                var opportunitySheet = excel.Workbook.Worksheets.Add("Danh sách cơ hội");
                opportunitySheet = ExcelExportHelper.GetStyle(opportunitySheet, 16);

                opportunitySheet.Cells[1, 1].Value = "Email của người nhận cơ hội";
                opportunitySheet.Cells[1, 2].Value = "Ngân sách";
                opportunitySheet.Cells[1, 3].Value = "Tên khách hàng";
                opportunitySheet.Cells[1, 4].Value = "Trạng thái";
                opportunitySheet.Cells[1, 5].Value = "Nhu cầu khách hàng";
                opportunitySheet.Cells[1, 6].Value = "Người phụ trách kỹ thuật";
                opportunitySheet.Cells[1, 7].Value = "Thời điểm dự kiến cơ hội diễn ra";
                opportunitySheet.Cells[1, 8].Value = "Giá vốn dự kiến";
                opportunitySheet.Cells[1, 9].Value = "Hoa hồng cho người tư vấn";
                opportunitySheet.Cells[1, 10].Value = "Đối thủ 1";
                opportunitySheet.Cells[1, 11].Value = "Đối thủ 2";
                opportunitySheet.Cells[1, 12].Value = "Chiến lược của công ty";
                opportunitySheet.Cells[1, 13].Value = "Lần tương tác gần nhất giữa AT&A và khách hàng";
                opportunitySheet.Cells[1, 14].Value = "Người quyết định";
                opportunitySheet.Cells[1, 15].Value = "Người thụ hưởng";
                opportunitySheet.Cells[1, 16].Value = "Đánh giá khả năng thắng";

                int currRow = 2;

                for (var index = 1; index <= 5; index++)
                {
                    opportunitySheet.Row(currRow).Height = 20;
                    opportunitySheet.Cells[currRow, 1].Value = $"nguyenvana{index}@gmail.com";
                    opportunitySheet.Cells[currRow, 2].Value = $"{index}0000000";
                    opportunitySheet.Cells[currRow, 3].Value = $"Khách hàng {index}";
                    opportunitySheet.Cells[currRow, 4].Value = $"CLOSE";
                    opportunitySheet.Cells[currRow, 5].Value = $"Nhu cầu {index}";
                    opportunitySheet.Cells[currRow, 6].Value = $"Người phụ trách {index}";
                    opportunitySheet.Cells[currRow, 7].Value = $"2024/03/03";
                    opportunitySheet.Cells[currRow, 8].Value = $"{index}000";
                    opportunitySheet.Cells[currRow, 9].Value = $"{index}000";
                    opportunitySheet.Cells[currRow, 10].Value = $"Đối thủ 1";
                    opportunitySheet.Cells[currRow, 11].Value = $"Đối thủ 2";
                    opportunitySheet.Cells[currRow, 12].Value = $"Chiến lược {index}";
                    opportunitySheet.Cells[currRow, 13].Value = $"2024/03/03";
                    opportunitySheet.Cells[currRow, 14].Value = $"Người quyết định {index}";
                    opportunitySheet.Cells[currRow, 15].Value = $"Người thụ hưởng {index}";
                    opportunitySheet.Cells[currRow, 16].Value = $"Khả năng thắng cao";

                    currRow++;
                }

                opportunitySheet.Cells.AutoFitColumns();
                #endregion

                #region Trạng thái
                var status = await Mediator.Send(new OpportunityStatus_GetAllQuery(new OpportunityStatusGetAllQueryRequest()));
                var statusSheet = excel.Workbook.Worksheets.Add("Danh sách trạng thái");
                statusSheet = ExcelExportHelper.GetStyle(statusSheet, 2);

                statusSheet.Cells[1, 1].Value = "Mã trạng thái";
                statusSheet.Cells[1, 2].Value = "Tên trạng thái";
                currRow = 2;

                foreach (var item in status.Data)
                {
                    statusSheet.Row(currRow).Height = 20;
                    statusSheet.Cells[currRow, 1].Value = item.Code;
                    statusSheet.Cells[currRow, 2].Value = item.Name;

                    currRow++;
                }

                statusSheet.Cells.AutoFitColumns();
                #endregion


                return File(excel.GetAsByteArray(), "application/vnd.ms-excel", String.Format("{0}.xlsx", "Danh sách cơ hội"));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/get-list")]
        public async Task<IActionResult> GetListWithPaginationByMobile([FromQuery] OpportunityGetListWithPaginationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                request.UserId = GetCurrentUser();

                return Ok(await Mediator.Send(new OpportunityMobile_GetListWithPaginationQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/get-by-role-type")]
        public async Task<IActionResult> GetListWithPaginationV2ByMobile([FromQuery] OpportunityGetListWithPaginationRequest request)
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

                return Ok(await Mediator.Send(new OpportunityMobile_GetListWithPaginationV2Query(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-list-user")]
        public async Task<IActionResult> MobileGetListUser([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                var result = await Mediator.Send(new OpportunityMobile_GetListUserQuery(userId, request));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/add")]
        public async Task<IActionResult> AddByMobile([FromBody] CreateOrUpdateOpportunityRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new OpportunityMobile_AddCommand(user, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-list-type-money")]
        public async Task<IActionResult> GetListTypeMoney()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new OpportunityMobile_GetListTypeMoneyQuery(user)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("mobile/update")]
        public async Task<IActionResult> UpdateByMobile([FromBody] CreateOrUpdateOpportunityRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new OpportunityMobile_UpdateCommand(user, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-by-id/{id}")]
        //[HasPermission(PolicyTypes.Sale_CH.DETAIL)]
        public async Task<IActionResult> GetByIdByMobile(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new OpportunityMobile_GetByIdQuery(userId, id)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/update-to-other")]
        [HasPermission(PolicyTypes.Sale_CH.CH_GANCOHOI)]
        public async Task<IActionResult> UpdateToOtherByMobile([FromBody] UpdateToOtherRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new OpportunityMobile_UpdateToOtherCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("opponent/delete-by-ids")]
        public async Task<IActionResult> MobileOpportunityOpponentDeleteByIds([FromBody] DeleteRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new OpportunityOpponentMobile_DeleteCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("history/delete-by-ids")]
        public async Task<IActionResult> MobileOpportunityHistoryDeleteByIds([FromBody] DeleteRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new OpportunityHistoryMobile_DeleteCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
