using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Sale_Saas.Infrastructure.Services.TenantService;
using Sale_Saas.Application.Interfaces.Services;
using System.Reflection;
using Sale_Saas.Application.Features.MailServerInfoFeature.Model;
using static System.Net.WebRequestMethods;
using Sale_Saas.Domain.Entities.Tenant;
namespace Sale_Saas.API.Controllers
{
    [AllowAnonymous]
    public class AuthenticationsController : BaseController
	{
        private readonly IApplicationUserService _applicationUserService;
        private readonly IUserService _userService;
        private readonly ILoggerService _loggerService;
        private readonly ISendEmailNotificationService _sendEmailNotificationService;
        private readonly IOtpSendService _otpSendService;
        public AuthenticationsController(IApplicationUserService applicationUserService, IUserService userService, ILoggerService loggerService, ISendEmailNotificationService sendEmailNotificationService, IOtpSendService otpSendService)
        {
            _applicationUserService = applicationUserService;
            _userService = userService;
            _loggerService = loggerService;
            _sendEmailNotificationService = sendEmailNotificationService;
            _otpSendService = otpSendService;
        }
        [HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userLogin=await _userService.LoginUser(request.UserName, request.Password,request.GroupTenantId, request.DeviceID ?? "");
                var tenant = Request.Query["tenant"].ToString();
                
                if(userLogin.Data == null)
                {
                    throw new ApplicationException("Đăng nhập thất bại");
                }
				if (userLogin.Data.UserTenantDtos.Any()) {
                    _applicationUserService.SetConnectDB(userLogin.Data.UserTenantDtos[0].ConnectString ?? "");
					tenant = userLogin.Data.UserTenantDtos[0].TenantId;
                    var groupTenant = userLogin.Data.GroupTenantId.ToString() ?? "";
					var resData = await _applicationUserService.Login(request, tenant, groupTenant);
					if (resData.Succeeded && resData.Data != null)
					{
						resData.Data.TenantIds = userLogin.Data.UserTenantDtos.Select(x => x.TenantId).ToList();
						resData.Data.IsAdmin = userLogin.Data.IsAdmin;
                        resData.Data.GroupTenantId = userLogin.Data.GroupTenantId;
					}
					return Ok(resData);
				}
                else
                {
                    var info = (await _userService.GetById(userLogin.Data.Id)).Data;
                    if (info == null) throw new ApplicationException("Không tìm thấy tài khoản");

					var resData = await _applicationUserService.LoginWithNoTenant(info);
					if (resData.Succeeded && resData.Data != null)
					{
                        resData.Data.TenantIds = new List<string>();
						resData.Data.IsAdmin = userLogin.Data.IsAdmin;
					}
					return Ok(resData);
				}
			    
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/login")]
        public async Task<IActionResult> MobileLogin([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userLogin = await _userService.LoginUser(request.UserName, request.Password, request.GroupTenantId, request.DeviceID ?? "");
                var tenant = Request.Query["tenant"].ToString();

                if (userLogin.Data == null)
                {
                    throw new ApplicationException("Đăng nhập thất bại");
                }
                if (userLogin.Data.UserTenantDtos.Any())
                {
                    _applicationUserService.SetConnectDB(userLogin.Data.UserTenantDtos[0].ConnectString ?? "");
                    tenant = userLogin.Data.UserTenantDtos[0].TenantId;
                    var groupTenant = userLogin.Data.GroupTenantId.ToString() ?? "";
                    var resData = await _applicationUserService.Login(request, tenant, groupTenant);
                    if (resData.Succeeded && resData.Data != null)
                    {
                        resData.Data.TenantIds = userLogin.Data.UserTenantDtos.Select(x => x.TenantId).ToList();
                        resData.Data.IsAdmin = userLogin.Data.IsAdmin;
                        resData.Data.GroupTenantId = userLogin.Data.GroupTenantId;
                    }
                     _userService.UpdateRefreshTokenUser(userLogin.Data.Id, resData.Data.RefreshToken);
                    return Ok(resData);
                }
                else
                {
                    var info = (await _userService.GetById(userLogin.Data.Id)).Data;
                    if (info == null) throw new ApplicationException("Không tìm thấy tài khoản");

                    var resData = await _applicationUserService.LoginWithNoTenant(info);
                    if (resData.Succeeded && resData.Data != null)
                    {
                        resData.Data.TenantIds = new List<string>();
                        resData.Data.IsAdmin = userLogin.Data.IsAdmin;
                    }
                    _userService.UpdateRefreshTokenUser(userLogin.Data.Id, resData.Data.RefreshToken);
                    return Ok(resData);
                }

            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/send-otp-register")]
        public async Task<IActionResult> SendOTP([FromBody] SendOtpRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userLogin = await _userService.GetUserByEmail(request.Email, request.GroupTenantId, request.DeviceID ?? "");
                if (userLogin !=null)
                {
                    throw new ApplicationException("Tài khoản thực sự đã tồn tại.");
                }
                Random rdn = new Random();
                string genOTP = rdn.Next(100000, 999999).ToString();
                var emailModel = new EmailMessage();
                emailModel.ToEmail = request.Email ?? "";
                emailModel.Subject = "Đăng ký tài khoản";
                emailModel.Body = $"Mã OTP của bạn là: <b>{genOTP}</b>";
                emailModel.IsHtml = true;
                var idOtp = Guid.NewGuid();
                var otpSend = await _otpSendService.Create(new OtpSend()
                {
                    Id = idOtp,
                    PhoneNo = emailModel.ToEmail,
                    Otp = genOTP,
                    Content = emailModel.Body,
                    ResultContent = "",
                    DeviceId = request.DeviceID ?? "",
                    DateInput = DateTime.Now,
                    ConfirmFlag = false
                });
                if (otpSend == null)
                    throw new ApplicationException("Không gửi được OTP.");
                _sendEmailNotificationService.SendEmailMessage(emailModel);
                return Ok(Result<ResponseSendOtp>.Success(new ResponseSendOtp() { IdOtp=idOtp}));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] SendOtpRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userLogin = await _userService.GetUserByEmail(request.Email, request.GroupTenantId,request.DeviceID ?? "");
                if (userLogin== null)
                {
                    throw new ApplicationException("Không tìm thấy tài khoản");
                }
                Random rdn = new Random();
                string genOTP = rdn.Next(100000, 999999).ToString();
                var emailModel = new EmailMessage();
                emailModel.ToEmail = userLogin.Email ?? "";
                emailModel.Subject = "Quên mật khẩu";
                emailModel.Body = $"Mã OTP của bạn là: <b>{genOTP}</b>";
                emailModel.IsHtml = true;
                var idOtp = Guid.NewGuid();
                var otpSend = await _otpSendService.Create(new OtpSend()
                {
                    Id= idOtp,
                    PhoneNo=emailModel.ToEmail,
                    Otp=genOTP,
                    Content=emailModel.Body,
                    ResultContent="",
                    DateInput=DateTime.Now,
                    ConfirmFlag=false
                });
                if(otpSend==null)
                    throw new ApplicationException("Không gửi được OTP.");
                _sendEmailNotificationService.SendEmailMessage(emailModel);
                return Ok(Result<ResponseSendOtp>.Success(new ResponseSendOtp() { IdOtp = idOtp }));

            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOTPRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userLogin = await _userService.GetUserByEmail(request.Email, request.GroupTenantId, request.DeviceID ?? "");
                if (userLogin == null)
                {
                    throw new ApplicationException("Không tìm thấy tài khoản");
                }
                var otpSend = await _otpSendService.GetOtpByEmail(request.Email, request.DeviceID ?? "", request.OtpCode);
                if (otpSend == null)
                    throw new ApplicationException("Mã OTP không tồn tại.");
                if(Math.Abs((otpSend.DateInput - DateTime.Now).Minutes) > 5)
                    throw new ApplicationException("OTP hết hạn.");
                var newPass = await _userService.UpdatePasswordGen(userLogin);
                return Ok(Result<string>.Success(newPass));

            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/refresh-token")]
        [Authorize]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userLogin =  _userService.GetUserByRefreshToken(request.RefreshToken);
                if (userLogin == null)
                {
                    throw new ApplicationException("Không thể khởi tạo token mới!");
                }               
                var token = GetCurrentToken();
                var newToken =  _applicationUserService.RefreshToken(token,7);
                var validTo=_applicationUserService.GetValidTo(newToken);
                var refreshToken = _applicationUserService.GenerateRefreshToken();
                return Ok(Result<ResponseRefreshToken>.Success(new ResponseRefreshToken { AccessToken=newToken, ValidTo=validTo, RefreshToken= refreshToken }));

            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
               
                var result = await _applicationUserService.Logout();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
