using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Infrastructure.Services.TenantService;
using System.Reflection;

namespace Sale_Saas.API.Controllers.Admin
{
	[Route("api/[controller]")]
	[ApiController]
	public class AdminRoleController : BaseController
	{
		private readonly IApplicationUserService _applicationUserService;
		private readonly IApplicationRoleService _roleService;
		private readonly ITenantService _tenantService;
		private readonly ILoggerService _loggerService;

		public AdminRoleController(IApplicationUserService applicationUserService, IApplicationRoleService roleService, 
										ITenantService tenantService, ILoggerService loggerService)
		{
			_applicationUserService = applicationUserService;
			_roleService = roleService;
			_tenantService = tenantService;
			_loggerService = loggerService;
		}

		[HttpGet("get-list-with-pagination")]
		public async Task<IActionResult> GetListWithPagination([FromQuery] GetListRoleWithPaginationQueryRequest request)
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

				return Ok(await _roleService.GetListWithPaginationQuery(request));
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
				request.UserId = GetCurrentUser();
				var tenant = _tenantService.GetTenantInfoByTenantId(request.TenantId ?? "");
				if (tenant == null) throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {request.TenantId}");
				_applicationUserService.SetConnectDB(tenant.ConnectionString ?? "");

				return Ok(await _roleService.GetAllQuery(request));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpPost("add-or-update-role-to-tenant")]
		public async Task<IActionResult> AddOrUpdateRoleToTenant([FromBody] List<AddOrUpdateRequest> request,[FromQuery] string TenantId)
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

				return Ok(await _roleService.AddOrUpdateAsync(request));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpDelete("delete-from-tenant-by-ids/{ids}/{ApplicationUserId}")]
		public async Task<IActionResult> DeleteByIds(string ids, Guid ApplicationUserId,string TenantId)
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

				var request = new DeleteRequest()
				{
					Ids = ids.Split(",").ToList(),
					ApplicationUserId = ApplicationUserId
				};

				return Ok(await _roleService.DeleteByIds(request));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}
	}
}
