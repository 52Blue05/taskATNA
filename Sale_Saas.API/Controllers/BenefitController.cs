using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.BenefitFeature.Commands;
using Sale_Saas.Application.Features.BenefitFeature.Queries;
using Sale_Saas.Application.Features.BenefitFeature.Requests;
using Sale_Saas.Application.Features.BenefitStatusFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Benefit;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Authentication;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BenefitController : BaseController
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly ILoggerService _loggerService;

        public BenefitController(IApplicationUserService applicationUserService, ILoggerService loggerService)
        {
            _applicationUserService = applicationUserService;
            _loggerService = loggerService;
        }

        [HttpPost("filter")]
        [HasPermission(PolicyTypes.Sale_QL.View)]
        public async Task<IActionResult> Filter([FromQuery] FilterQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Benefit_GetListQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-all")]
        [HasPermission(PolicyTypes.Sale_QL.View)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Benefit_GetAllQuery(userId, request)));
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
                //request.UserId = GetCurrentUser();
                var userId = GetCurrentUser() ?? Guid.Empty;
                if (request.RoleType == RoleType.MYSELF.ToString())
                {
                    request.UserId = userId;
                    request.PageIndex = 1;
                    request.PageSize = 1;
                }
                else if (userId == request.UserId)
                {
                    //Website luôn truyền UserId=user login -> cần set về null
                    request.UserId = null;
                }

                // add temp (FE work => re-handle)
                if (request.RoleType == RoleType.EMPLOYEE.ToString())
                {
                    request.RoleId = null;
                }
                //return Ok(await Mediator.Send(new Benefit_GetListWithPaginationQuery(userId, request)));
                return Ok(await Mediator.Send(new Benefit_GetListWithPaginationQueryV2(userId, request)));
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
                    request.PageIndex = 1;
                    request.PageSize = 1;
                }
                else if (userId == request.UserId)
                {
                    request.UserId = null;
                }

                //return Ok(await Mediator.Send(new Benefit_GetListWithPaginationQuery(userId, request)));
                return Ok(await Mediator.Send(new Benefit_GetListWithPaginationQueryV2(userId, request)));

            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-by-role-type-v2")]
        public async Task<IActionResult> MobileGetListMySelfWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
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
                    //request.PageIndex = 1;
                    //request.PageSize = 1;
                }
                else if (userId == request.UserId)
                {
                    request.UserId = null;
                }

                //return Ok(await Mediator.Send(new Benefit_GetListWithPaginationQuery(userId, request)));
                return Ok(await Mediator.Send(new Benefit_GetListWithPaginationQueryV2(userId, request)));

            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-list-employee")]
        public async Task<IActionResult> MobileGetListEmployeHasBenefit([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new BenefitMobile_GetListEmployeeQuery(userId, request)));

            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-by-id/{id}")]
        [HasPermission(PolicyTypes.Sale_QL.DETAIL)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Benefit_GetByIdQuery(userId, id)));
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
                return Ok(await Mediator.Send(new Benefit_AddOrUpdateCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        // ============================= VERSION2 TESTED ======================================
        #region ============================= VERSION2 TESTED ======================================

        [HttpPost("add-or-update-V2")]
        public async Task<IActionResult> AddOrUpdateV2([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new Benefit_AddOrUpdate_V2Command(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/add-V2")]
        public async Task<IActionResult> MobileAddOrUpdateV2([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new Benefit_AddOrUpdate_V2Command(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-user-V2")]
        [HasPermission(PolicyTypes.Sale_QL.CREATE)]
        public async Task<IActionResult> GetUserV2([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                request.UserId = GetCurrentUser();
                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new Benefit_GetUser_V2Query(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        #endregion ============================= VERSION2 TESTED ======================================
        // ============================= VERSION2 TESTED ======================================

        [HttpPost("mobile/add")]
        public async Task<IActionResult> MobileAddOrUpdate([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new Benefit_AddOrUpdate_V2Command(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-users")]
        [HasPermission(PolicyTypes.Sale_QL.CREATE)]
        public async Task<IActionResult> MobileGetListUser([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                request.UserId = GetCurrentUser();
                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new Benefit_GetUser_V2Query(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-role-by-user")]
        [HasPermission(PolicyTypes.Sale_QL.CREATE)]
        public async Task<IActionResult> GetRoleByUserHasNotBenefit([FromQuery] Guid UserId, [FromQuery] string RolePositionId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Benefit_GetListRoleOfUserNotExistBenefitQuery(UserId, RolePositionId)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
        [HasPermission(PolicyTypes.Sale_QL.DELETE)]
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

                return Ok(await Mediator.Send(new Benefit_DeleteByIdCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("update-status-by-id")]
        public async Task<IActionResult> UpdateStatusById([FromBody] UpdateStatusBenefitRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Benefit_UpdateStatusByIdCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("mobile/update-by-id")]
        public async Task<IActionResult> UpdateBenefitById([FromBody] UpdateStatusBenefitRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new BenefitMobile_UpdateStatusCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("update-total-benefit")]
        [HasPermission(PolicyTypes.Sale_QL.UPDATE_RESULT)]
        public async Task<IActionResult> UpdateTotalBenefit([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Benefit_UpdateTotalBenefitCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-user")]
        [HasPermission(PolicyTypes.Sale_QL.CREATE)]
        public async Task<IActionResult> GetUser([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                request.UserId = GetCurrentUser();
                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new Benefit_GetUserQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/add-with-fluctuation")]
        //[HasPermission(PolicyTypes.Sale_QL.CREATE)]
        public async Task<IActionResult> MobileAddV3([FromBody] BenefitMobileAddV3Request request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new BenefitMobile_AddV3Command(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("mobile/update-with-fluctuation")]
        //[HasPermission(PolicyTypes.Sale_QL.CREATE)]
        public async Task<IActionResult> MobileUpdateWithFluctuationV3([FromBody] BenefitMobileUpdateEstimateBenefitRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new BenefitMobile_UpdateEstimatedBenefitCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        #region ====================== Version 3 - Website ======================

        [HttpPost("add-with-fluctuation")]
        [HasPermission(PolicyTypes.Sale_QL.CREATE)]
        public async Task<IActionResult> AddWithFluctuationV3([FromBody] BenefitMobileAddV3Request request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new Benefit_AddV3Command(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("update-estimate-benefit-with-fluctuation")]
        //[HasPermission(PolicyTypes.Sale_QL.CREATE)]
        public async Task<IActionResult> UpdateEstimateBenefitWithFluctuationV3([FromBody] BenefitUpdateEstimateBenefitWithFluctuationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new Benefit_UpdateEstimateBenefitWithFluctuationCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-by-role-type")]
        public async Task<IActionResult> GetListWithPaginationV3([FromQuery] GetListWithPaginationQueryRequest request)
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
                    request.PageIndex = 1;
                    request.PageSize = 5;
                }
                else if (userId == request.UserId)
                {
                    request.UserId = null;
                }

                //return Ok(await Mediator.Send(new Benefit_GetListWithPaginationQuery(userId, request)));
                return Ok(await Mediator.Send(new Benefit_GetListWithPaginationV3Query(userId, request)));

            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        #endregion ==============================================================


        [HttpPost("import")]
        [HasPermission(PolicyTypes.Sale_QL.IMPORT_EXCEL)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                List<Benefit> listBenefits = new List<Benefit>();
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
                            var empBenefit = new Benefit();
                            empBenefit.Id = Guid.NewGuid();
                            empBenefit.DeleteFlag = false;
                            empBenefit.CreatedDate = DateTime.Now;
                            empBenefit.LastModifiedDate = DateTime.Now;
                            empBenefit.CreatedApplicationUserId = loginUserId;
                            empBenefit.LastModifiedApplicationUserId = loginUserId;
                            empBenefit.SuggestTotalSalary = 0;
                            empBenefit.SuggestMonthlySalary = 0;
                            empBenefit.SuggestTargetSalary = 0;

                            for (int column = 1; column <= columnCount; column++)
                            {
                                if (worksheet.Cells[row, column].Value.ToString() != null)
                                {
                                    switch (worksheet.Cells[1, column].Value.ToString())
                                    {
                                        case "Email của người hưởng quyền lợi":
                                            var userId = await _applicationUserService.GetUserIdByEmail(worksheet.Cells[row, column].Value.ToString() ?? "");
                                            if (userId.Data == Guid.Empty)
                                                return Ok(Result<string>.Failure($"Không tìm thấy nhân viên với email: {worksheet.Cells[row, column].Value.ToString()}"));
                                            empBenefit.ApplicationUserId = userId.Data;
                                            break;
                                        case "Mức lương cố định hàng tháng":
                                            empBenefit.MonthlySalary = int.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Tổng mức lương biến động mục tiêu":
                                            empBenefit.TargetSalary = int.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Trạng thái":
                                            var tempBenefitStatus = await Mediator.Send(new BenefitStatus_GetByNameQuery(loginUserId, worksheet.Cells[row, column].Value.ToString() ?? "0"));
                                            if (tempBenefitStatus.Data == null)
                                                return Ok(Result<string>.Failure("Trạng thái của quyền lợi khác trạng thái mẫu"));
                                            empBenefit.BenefitStatusId = tempBenefitStatus.Data.Id;
                                            break;
                                        case "Tổng quyền lợi hiện tại":
                                            empBenefit.TotalBenefit = int.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Mức lương biến động mục tiêu thực tế":
                                            empBenefit.TotalBenefit = int.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                    }
                                }
                            }
                            listBenefits.Add(empBenefit);
                        }
                    }
                }
                if (listBenefits.Count > 0)
                {
                    var userId = GetCurrentUser() ?? Guid.Empty;
                    return Ok(await Mediator.Send(new Benefit_ImportCommand(userId, listBenefits)));
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
        [HasPermission(PolicyTypes.Sale_QL.EXPORT_EXCEL)]
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
                var response = await Mediator.Send(new Benefit_GetListWithPaginationQuery(userId, request));
                if (response.Succeeded == false || response.Data == null)
                    throw new ApplicationException("Xuất file excel thất bại!");
                var data = response.Data.Items;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string locale = GetLocale();
                string listName = locale == LocaleEnum.vi_VN.ToString() ? "Danh sách quyền lợi" : "List benefits";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 6);

                workSheet.Cells[1, 1].Value = "NGƯỜI HƯỞNG";
                workSheet.Cells[1, 2].Value = "TỔNG QUYỀN LỢI HIỆN TẠI";
                workSheet.Cells[1, 3].Value = "MỨC LƯƠNG CỐ ĐỊNH HÀNG THÁNG";
                workSheet.Cells[1, 4].Value = "TỔNG MỨC LƯƠNG BIẾN ĐỘNG MỤC TIÊU";
                workSheet.Cells[1, 5].Value = "TỔNG MỨC LƯƠNG BIẾN ĐỘNG THỰC TẾ";
                workSheet.Cells[1, 6].Value = "TRẠNG THÁI";

                int currRow = 2;

                foreach (var item in data)
                {
                    workSheet.Row(currRow).Height = 20;
                    workSheet.Cells[currRow, 1].Value = item.ApplicationUser.FullName;
                    workSheet.Cells[currRow, 2].Value = item.TotalBenefit;
                    workSheet.Cells[currRow, 3].Value = item.MonthlySalary;
                    workSheet.Cells[currRow, 4].Value = item.TargetSalary;
                    workSheet.Cells[currRow, 5].Value = item.TotalSalary;
                    workSheet.Cells[currRow, 6].Value = item.BenefitStatus.Name;

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
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                #region MỤC TIÊU
                var benefitSheet = excel.Workbook.Worksheets.Add("Danh sách quyền lợi");
                benefitSheet = ExcelExportHelper.GetStyle(benefitSheet, 6);

                benefitSheet.Cells[1, 1].Value = "Email của người hưởng quyền lợi";
                benefitSheet.Cells[1, 2].Value = "Mức lương cố định hàng tháng";
                benefitSheet.Cells[1, 3].Value = "Tổng mức lương biến động mục tiêu";
                benefitSheet.Cells[1, 4].Value = "Tổng quyền lợi hiện tại";
                benefitSheet.Cells[1, 5].Value = "Mức lương biến động mục tiêu thực tế";
                benefitSheet.Cells[1, 6].Value = "Trạng thái";

                int currRow = 2;

                for (var index = 1; index <= 5; index++)
                {
                    benefitSheet.Row(currRow).Height = 20;
                    benefitSheet.Cells[currRow, 1].Value = $"nguyenvana{index}@gmail.com";
                    benefitSheet.Cells[currRow, 2].Value = $"{index}00";
                    benefitSheet.Cells[currRow, 3].Value = $"{index}00";
                    benefitSheet.Cells[currRow, 4].Value = $"{index}00";
                    benefitSheet.Cells[currRow, 5].Value = $"{index}00";
                    benefitSheet.Cells[currRow, 6].Value = "CONFIRMED";

                    currRow++;
                }

                benefitSheet.Cells.AutoFitColumns();
                #endregion

                #region Trạng thái
                var userId = GetCurrentUser() ?? Guid.Empty;
                var status = await Mediator.Send(new BenefitStatus_GetAllQuery(userId, new GetAllQueryRequest()));
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


                return File(excel.GetAsByteArray(), "application/vnd.ms-excel", String.Format("{0}.xlsx", "Danh sách quyền lợi"));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
