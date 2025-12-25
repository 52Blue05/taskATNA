using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Services.TenantService;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;
using System.Data;
using System.Drawing;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    public class ApplicationUsersController : BaseController
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IApplicationRoleService _roleService;
        private readonly ISendEmailNotificationService _sendEmailNotificationService;
        private readonly ITenantService _tenantService;
        private readonly ILoggerService _loggerService;

        public ApplicationUsersController(IApplicationUserService applicationUserService,
                                            IConfiguration configuration, ITenantService tenantService,
                                            IUserService userService, ISendEmailNotificationService sendEmailNotificationService,
                                            IApplicationRoleService roleService,
                                            ILoggerService loggerService)
        {
            _applicationUserService = applicationUserService;
            _configuration = configuration;
            _userService = userService;
            _tenantService = tenantService;
            _sendEmailNotificationService = sendEmailNotificationService;
            _roleService = roleService;
            _loggerService = loggerService;
        }

        [HttpPut("update-employee")]
        public async Task<IActionResult> UpdateEmployee([FromBody] List<ApplicationUserDto> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await _applicationUserService.UpdateEmployeeAsync(request, GetCurrentUser()));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("update-review-employee")]
        public async Task<IActionResult> UpdateReviewEmployee([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await _applicationUserService.UpdateReviewEmployeeAsync(request, GetCurrentUser()));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("add-or-update-without-files")]
        public async Task<IActionResult> AddOrUpdateWithoutFiles([FromBody] List<AddOrUpdateApplicationUserRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await _applicationUserService.AddOrUpdateAsync(request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-roles")]
        public async Task<IActionResult> GetListRoles()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await _roleService.GetListRoleByUserIdHasResult(userId));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/update-avatar")]
        public async Task<IActionResult> UpdateAvatar([FromForm] UpdateAvatarRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = await _userService.GetUserByEmail(request.Email, request.GroupTenantId, request.DeviceID ?? "");
                if (user == null)
                {
                    throw new ApplicationException("Không tìm thấy tài khoản");
                }

                if (request.Avatar == null)
                {
                    throw new ApplicationException("Hình ảnh gửi lên máy chủ bị rỗng");
                }

                return Ok(await _userService.UpdateAvatar(user, request.Avatar));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("get-profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var user = GetCurrentUser();
                if (user == null)
                {
                    var userAdmin = GetCurrentUserAdmin();
                    if (userAdmin == null)
                    {
                        return Ok(Result<string>.Failure("Tài khoản không tồn tại"));
                    }
                    var dataAdmin = await _applicationUserService.GetProfileAdmin((string)userAdmin);
                    return Ok(dataAdmin);
                }


                var data = await _applicationUserService.GetProfile((Guid)user);
                if (data.Data != null)
                {
                    data.Data.CurrentTenant = GetCurrentTenant();
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-profile")]
        public async Task<IActionResult> GetProfileByMobile()
        {
            try
            {
                var user = GetCurrentUser();
                if (user == null)
                {
                    var userAdmin = GetCurrentUserAdmin();
                    if (userAdmin == null)
                    {
                        return Ok(Result<string>.Failure("Tài khoản không tồn tại"));
                    }
                    var dataAdmin = await _applicationUserService.GetProfileAdmin((string)userAdmin);
                    return Ok(dataAdmin);
                }


                var data = await _applicationUserService.GetProfile((Guid)user);
                if (data.Data != null)
                {
                    data.Data.CurrentTenant = GetCurrentTenant();
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("add-user")]
        public async Task<IActionResult> AddOrUpdateHasFiles([FromBody] AddOrUpdateApplicationUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                //if (request.Data == null && !string.IsNullOrEmpty(request.FormUrlEncodedContent_Data))
                //{
                //    var reader = new FormReader(request.FormUrlEncodedContent_Data);
                //    request.Data = reader.ReadForm().ToDictionary(m => m.Key, m => m.Value.ToString());
                //}
                List<AddOrUpdateApplicationUserRequest> addOrUpdateApplicationUserRequests = new List<AddOrUpdateApplicationUserRequest>();
                addOrUpdateApplicationUserRequests.Add(request);

                return Ok(await _applicationUserService.AddOrUpdateAsync(addOrUpdateApplicationUserRequests));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpPost("save-applicationrole")]
        public async Task<IActionResult> SaveApplicationRoles([FromBody] ApplicationUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await _applicationUserService.SaveApplicationRolesAsync(request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await _applicationUserService.ChangePasswordAsync(request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await _applicationUserService.ResetPasswordAsync(request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-list-with-pagination")]
        public async Task<IActionResult> GetListWithPagination([FromQuery] GetListApplicationUserWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                request.UserId = GetCurrentUser();
                return Ok(await _applicationUserService.GetListWithPaginationQuery(request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-user-in-tenant-pagination")]
        public async Task<IActionResult> GetUserInTenant([FromQuery] GetListApplicationUserWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                request.UserId = GetCurrentUser();
                var tenant = _tenantService.GetTenantInfoByTenantId(request.TenantId ?? "");
                if (tenant == null) throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {request.TenantId}");
                _applicationUserService.SetConnectDB(tenant.ConnectionString ?? "");
                return Ok(await _applicationUserService.GetUserInTenantPaginationQuery(request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-user-in-tenant-pagination-v2")]
        public async Task<IActionResult> GetUserInTenantV2([FromQuery] GetListApplicationUserWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var username = GetCurrentUserName();
                var tenant = request.TenantId;

                request.UserId = GetCurrentUser();
                return Ok(await _applicationUserService.GetUserInTenantPaginationQuery(request));
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

                return Ok(await _applicationUserService.GetAllQueryAsync(request));
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

                return Ok(await _applicationUserService.GetById(id));
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

                return Ok(await _applicationUserService.DeleteByIds(request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpDelete("delete-avatar-by-applicationuserid/{applicationUserId}")]
        public async Task<IActionResult> DeleteAvatarByUserId(string applicationUserId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await _applicationUserService.DeleteAvatarByApplicationUserId(applicationUserId));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] FilterQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await _applicationUserService.FilterQueryAsync(request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-list-tenants-by-current-user")]
        public async Task<IActionResult> GetListTenantsByCurrentUserId()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var id = GetCurrentUser() ?? Guid.Empty;

                var result = await _userService.ListUserTenantByMobile(id, false);

                return Ok(Result<List<UserTenantMobileDto>>.Success(result));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("mobile/change-password-by-current-user")]
        public async Task<IActionResult> ChangePasswordByCurrentUserId([FromBody] UpdatePasswordMobileRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await _userService.UpdatePasswordByMobile(request, userId, true));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("mobile/update-avatar-by-current-user")]
        public async Task<IActionResult> UpdateAvatarByCurrentUser(IFormFile avatar)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await _userService.UpdateAvatarByMobile(userId, avatar));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        //[HttpPost("import")]
        //public async Task<IActionResult> Import(IFormFile file)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }
        //        List<ApplicationUserTenant> listUserTenants = new List<ApplicationUserTenant>();
        //        var loginUserId = GetCurrentUser();
        //        using (var stream = new MemoryStream())
        //        {
        //            await file.CopyToAsync(stream);
        //            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //            using (var package = new ExcelPackage(stream))
        //            {
        //                var worksheet = package.Workbook.Worksheets[0];
        //                var rowcount = worksheet.Dimension.Rows;
        //                var columnCount = worksheet.Dimension.Columns;
        //                for (int row = 2; row <= rowcount; row++)
        //                {
        //                    var empUsers = new ApplicationUser();
        //                    empcontract.Id = Guid.NewGuid();
        //                    empcontract.DeleteFlag = false;
        //                    empcontract.CreatedDate = DateTime.Now;
        //                    empcontract.LastModifiedDate = DateTime.Now;
        //                    empcontract.CreatedApplicationUserId = loginUserId;
        //                    empcontract.LastModifiedApplicationUserId = loginUserId;
        //                    for (int column = 1; column <= columnCount; column++)
        //                    {
        //                        if (worksheet.Cells[row, column].Value.ToString() != null)
        //                        {
        //                            switch (worksheet.Cells[1, column].Value.ToString())
        //                            {
        //                                case "Mã HD":
        //                                    empcontract.Code = worksheet.Cells[row, column].Value.ToString();
        //                                    break;
        //                                case "Số HD":
        //                                    empcontract.Number = worksheet.Cells[row, column].Value.ToString();
        //                                    break;
        //                                case "Tên HD":
        //                                    empcontract.Name = worksheet.Cells[row, column].Value.ToString();
        //                                    break;
        //                                case "Ngày bắt đầu":
        //                                    empcontract.StartDate = DateTime.Parse(worksheet.Cells[row, column].Value.ToString());
        //                                    break;
        //                                case "Ngày kết thúc":
        //                                    empcontract.EndDate = DateTime.Parse(worksheet.Cells[row, column].Value.ToString());
        //                                    break;
        //                                case "Mã KH":
        //                                    var customer = await Mediator.Send(new Customer_GetByCodeQuery(worksheet.Cells[row, column].Value.ToString()));
        //                                    if (customer.Data == null)
        //                                        return Ok(Result<string>.Failure("Mã KH khác mã KH mẫu!"));
        //                                    empcontract.CustomerId = customer.Data.Id;
        //                                    break;
        //                                case "Trạng thái":
        //                                    var tempContractStatus = await Mediator.Send(new ContractStatus_GetByNameQuery(worksheet.Cells[row, column].Value.ToString()));
        //                                    if (tempContractStatus.Data == null)
        //                                        return Ok(Result<string>.Failure("Trạng thái của hợp đồng khác trạng thái mẫu!"));
        //                                    empcontract.ContractStatusId = tempContractStatus.Data.Id;
        //                                    break;
        //                            }
        //                        }
        //                    }
        //                    listContracts.Add(empcontract);
        //                }
        //            }
        //        }
        //        if (listContracts.Count > 0)
        //        {
        //            return Ok(await Mediator.Send(new Contract_ImportCommand(listContracts)));
        //        }

        //        return Ok(Result<string>.Failure("File import khác file mẫu!"));
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(Result<string>.Failure(ex.Message));
        //    }
        //}

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] GetListApplicationUserWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                request.UserId = GetCurrentUser();
                var datas = await _applicationUserService.GetListWithPaginationQuery(request);
                #region ======= excel =======
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();

                string locale = GetLocale();
                string listName = locale == LocaleEnum.vi_VN.ToString() ? "Danh sách nhân sự" : "List Users";

                // name of the sheet 
                var workSheet = excel.Workbook.Worksheets.Add(listName);

                // setting the properties 
                // of the work sheet  
                workSheet.TabColor = System.Drawing.Color.Black;
                //border
                workSheet.Cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                workSheet.Cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                workSheet.Cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                workSheet.Cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                //center vertical
                workSheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                //freeze top row
                workSheet.View.FreezePanes(2, 1);

                //color columns
                for (int i = 1; i <= 7; i++)
                {
                    workSheet.Column(i).Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Column(i).Style.Fill.BackgroundColor.SetColor(Color.FromArgb(237, 237, 237));
                    workSheet.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Setting the properties 
                // of the first row 
                workSheet.Row(1).Height = 30;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;
                workSheet.Row(1).Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Row(1).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#305496"));
                workSheet.Row(1).Style.Font.Color.SetColor(ColorTranslator.FromHtml("#FFFFFF"));

                // Header of the Excel sheet 
                //pipe
                workSheet.Cells[1, 1].Value = "ID";
                workSheet.Cells[1, 2].Value = "MÃ NHÂN SỰ";
                workSheet.Cells[1, 3].Value = "HỌ VÀ TÊN";
                workSheet.Cells[1, 4].Value = "VỊ TRÍ";
                workSheet.Cells[1, 5].Value = "NGÀY SINH";
                workSheet.Cells[1, 6].Value = "ĐỊA CHỈ";
                workSheet.Cells[1, 7].Value = "EMAIL";
                workSheet.Cells[1, 8].Value = "SỐ ĐIỆN THOẠI";

                if (locale != LocaleEnum.vi_VN.ToString())
                {
                    workSheet.Cells[1, 2].Value = "CODE";
                    workSheet.Cells[1, 3].Value = "FULL NAME";
                    workSheet.Cells[1, 4].Value = "POSITION";
                    workSheet.Cells[1, 5].Value = "DATE OF BIRTH";
                    workSheet.Cells[1, 6].Value = "ADDRESS";
                    workSheet.Cells[1, 8].Value = "NUMBER PHONE";
                }

                int currRow = 2;

                foreach (var user in datas.Data!.Items)
                {
                    workSheet.Row(currRow).Height = 20;
                    workSheet.Cells[currRow, 1].Value = user.Id;
                    workSheet.Cells[currRow, 2].Value = user.Code;
                    workSheet.Cells[currRow, 3].Value = user.FullName;
                    workSheet.Cells[currRow, 4].Value = user.ApplicationRoles != null ? string.Join(" - ", user.ApplicationRoles.Select(role => role.DisplayName)) : "";
                    workSheet.Cells[currRow, 5].Value = user.DateOfBirth.HasValue ? user.DateOfBirth.Value.ToString("dd/MM/yyyy") : "";
                    workSheet.Cells[currRow, 6].Value = "";
                    workSheet.Cells[currRow, 7].Value = user.Email;
                    workSheet.Cells[currRow, 8].Value = user.Phone;

                    currRow++;
                }

                workSheet.Cells.AutoFitColumns();
                #endregion
                return File(excel.GetAsByteArray(), "application/vnd.ms-excel", String.Format("{0}.xlsx", listName));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        //[HttpPost("email")]
        //[AllowAnonymous]
        //public async Task<IActionResult> Email()
        //{
        //    var emailModel = new EmailMessage();
        //    emailModel.ToEmail = "lanhoang200917@gmail.com";
        //    emailModel.Subject = "SunShine Software";
        //    emailModel.Body = "$@\"<html>\r\n    <head></head>\r\n    <body style= \"\"margin:0;padding:0;font-family: Arial, Helvetica, sans-serif;\"\">\r\n        <div style=\"\"height:auto; background: #009bab; width:400px;padding:30px\"\">\r\n            <div>\r\n                <div>\r\n                    <h1 style=\"\"color:white\"\">Reset your password</h1>\r\n                    <hr>\r\n                    <p style=\"\"color:white\"\">You're receiving this email because you requested a password reset for your KT-eHospital account.</p>\r\n                    <p style=\"\"color:white\"\">Please tap the button below to choose a new password</p>\r\n                    <a href=\"\"http://localhost:4200/resetpassword\"\" , target=\"\"_blank\"\" \r\n                        style=\"\"background:white; padding:10px; border:none; color:#009bab; border-radius:4px; display:block;\r\n                           margin:0 auto; width:50%; text-align:center; text-decoration:none\"\">Reset Password</a><br>\r\n                    <p style=\"\"color:white; text-align: right\"\">Best Regards,<br><br>\r\n                    KT-eHospital</p>\r\n                </div>\r\n            </div>\r\n        </div>  \r\n    </body>\r\n</html>\"";
        //    emailModel.IsHtml = true;

        //    _sendEmailNotificationService.SendEmailMessage(emailModel);

        //    return Ok();
        //}
    }
}