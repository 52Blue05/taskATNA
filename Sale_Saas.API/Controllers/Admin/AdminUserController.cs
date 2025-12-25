using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Infrastructure.Services.TenantService;
using System.Reflection;

namespace Sale_Saas.API.Controllers.Admin
{
	[Route("api/[controller]")]
	[ApiController]
	public class AdminUserController : BaseController
	{
		private readonly IConfiguration _configuration;
		private readonly IUserService _userService;
		private readonly IApplicationUserService _applicationUserService;
		private readonly ITenantService _tenantService;
		private readonly ILoggerService _loggerService;

		public AdminUserController( IConfiguration configuration,IUserService userService, IApplicationUserService applicationUserService, 
										ITenantService tenantService, ILoggerService loggerService)
		{
			_configuration = configuration;
			_userService = userService;
			_applicationUserService = applicationUserService;
			_tenantService = tenantService;
			_loggerService = loggerService;
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
				request.GroupTenantId = GetGroupTenant();
				return Ok(await _userService.GetListWithPaginationQuery(request));
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
				request.UserId = GetCurrentUser();
				request.GroupTenantId = GetGroupTenant();
				return Ok(await _userService.GetListWithFilterQuery(request));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpGet("get-by-id")]
		public async Task<IActionResult> GetById(Guid id)
		{
			try
			{
				var result = await _userService.GetById(id);
				return Ok(result);
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpPut("update-status")]
		public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				request.ApplicationUserId = GetCurrentUser() ?? Guid.Empty;
				return Ok(await _userService.UpdateStatus(request));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpPut("update-password")]
		public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
                request.id = GetCurrentUser() ?? Guid.Empty;
                return Ok(await _userService.UpdatePassword(request,false));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

        [HttpPut("mobile/update-password")]
        public async Task<IActionResult> UpdatePasswordByMobile([FromBody] UpdatePasswordByOtpMobileRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await _userService.UpdatePasswordByOtpMobile(request, userId, true));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("add-user")]
		public async Task<IActionResult> Add([FromBody] List<AddOrUpdateRequest> request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				var userId= GetCurrentUser()??Guid.Empty;
                return Ok(await _userService.AddUser(request,GetGroupTenant(),userId));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpPut("update-user")]
		public async Task<IActionResult> Update([FromBody] List<AddOrUpdateRequest> request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await _userService.UpdateUser(request,userId));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpPut("update-profile")]
		public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request)
		{
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await _userService.UpdateProfile(request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

		[HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
		public async Task<IActionResult> DeleteByIds(string ids, Guid ApplicationUserId, string? Locale = "vi_VN")
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var user = GetCurrentUser() ?? Guid.Empty;


				var request = new DeleteRequest()
				{
					Ids = ids.Split(",").ToList(),
					ApplicationUserId = ApplicationUserId,
					Locale = Locale
				};

				return Ok(await _userService.DeleteByIds(request));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpGet("get-list-with-pagination-and-tenant")]
		public async Task<IActionResult> GetListWithPaginationAndTenant([FromQuery] GetListApplicationUserWithPaginationQueryRequest request)
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
				return Ok(await _applicationUserService.GetListWithPaginationQuery(request));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpGet("recommend")]
		public async Task<IActionResult> Recommend([FromQuery] FilterQueryRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				request.UserId = GetCurrentUser();
				request.GroupTenantId = GetGroupTenant();
				return Ok(await _userService.GetRecommendUserQuery(request));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpGet("get-by-id-with-tenant")]
		public async Task<IActionResult> GetByIdWithTenant(Guid id,string TenantId)
		{
			try
			{
				var result = await _userService.GetById(id);

				var tenant = _tenantService.GetTenantInfoByTenantId(TenantId ?? "");
				if (tenant == null) throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {TenantId}");
				_applicationUserService.SetConnectDB(tenant.ConnectionString ?? "");

				return Ok(await _applicationUserService.GetById(id));
			}
			catch(Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
			
		}

		[HttpPost("add-user-with-role-to-tenant")]
		public async Task<IActionResult> AddUserWithRole([FromBody] AddUserToTenantDto request, [FromQuery] string TenantId)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				request.TenantId = TenantId;
				var tenant = _tenantService.GetTenantInfoByTenantId(TenantId ?? "");
				if (tenant == null) throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {TenantId}");
				_applicationUserService.SetConnectDB(tenant.ConnectionString ?? "");
				var userId=GetCurrentUser()??Guid.Empty;
				return Ok(await _userService.AddToTenant(request, userId));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpPut("update-user-with-role-to-tenant")]
		public async Task<IActionResult> UpdateUserWithRole([FromBody] UpdateUserToTenantDto request, [FromQuery] string TenantId)
		{
			try
			{
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
				request.TenantId = TenantId;
                var tenant = _tenantService.GetTenantInfoByTenantId(TenantId ?? "");
                if (tenant == null) throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {TenantId}");
                _applicationUserService.SetConnectDB(tenant.ConnectionString ?? "");
                var currentUserName = GetCurrentUserName();
                var userCurrent = _userService.GetUserAdminByUserName(currentUserName);
                
                var AssignRoleRequest = new ApplicationRoleAssignRequest
                {
                    Id = request.Id,
                    ApplicationRoleIds = request.ApplicationRoleIds,
					IsAdmin=userCurrent!=null?true : false,				
                };
                return Ok(await _applicationUserService.UpdateUserWithManyRole(AssignRoleRequest));
            }
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpDelete("delete-from-tenant-by-ids/{ids}/{ApplicationUserId}")]
		public async Task<IActionResult> DeleteFromTenantByIds(string ids, Guid ApplicationUserId,string TenantId)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				var tenant = _tenantService.GetTenantInfoByTenantId(TenantId ?? "");
				if (tenant == null) throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {TenantId}");
				_applicationUserService.SetConnectDB(tenant.ConnectionString ?? "");
				var user = GetCurrentUser() ?? Guid.Empty;

				var request = new DeleteRequest()
				{
					Ids = ids.Split(",").ToList(),
					ApplicationUserId = ApplicationUserId,
					TenantId = TenantId,
					Locale = GetLocale()
				};

				var result = await _applicationUserService.DeleteByIds(request);
				if (result.Succeeded)
				{
					result = await _userService.DeleteFromTenantByIds(request);
				}
				return Ok(result);
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpGet("status/get-all")]
		public async Task<IActionResult> GetAllStatus()
		{
            try
            {
                var result = await _userService.GetAllStatus();
                
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
