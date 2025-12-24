using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.FeaturePermissionFeature.Commands;
using Sale_Saas.Application.Features.FeaturePermissionFeature.Dto;
using Sale_Saas.Application.Features.FeaturePermissionFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Infrastructure.Services.Identity;
using Sale_Saas.Infrastructure.Services.TenantService;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class FeaturePermissionController : BaseController
	{
		private readonly IApplicationUserService _applicationUserService;
		private readonly IFeaturePermissionService _permissionService;
		private readonly ITenantService _tenantService;
		private readonly ILoggerService _loggerService;

		public FeaturePermissionController(IFeaturePermissionService permissionService, ITenantService tenantService, 
												IApplicationUserService applicationUserService, ILoggerService loggerService)
		{
			_permissionService = permissionService;
			_tenantService = tenantService;
			_applicationUserService = applicationUserService;
			_loggerService = loggerService;
		}

		[HttpPost("update")]
		public async Task<IActionResult> Update(List<MenuWithFeatureDto> request,Guid RoleId,string RolePositionId,string tenantId)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var tenant = _tenantService.GetTenantInfoByTenantId(tenantId ?? "");
				if (tenant == null) throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {tenantId}");
				_applicationUserService.SetConnectDB(tenant.ConnectionString ?? "");

				var userId = GetCurrentUser() ?? Guid.Empty;
				return Ok(await Mediator.Send(new FeaturePermission_UpdateCommand(userId, request,RoleId, RolePositionId)));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpGet("get-all-with-menu")]
		public async Task<IActionResult> GetAllListWithMenu([FromQuery] GetAllQueryRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var userId = GetCurrentUser() ?? Guid.Empty;
				var data = await Mediator.Send(new FeaturePermission_GetAllQuery(userId, request));
				return Ok(data);
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpGet("get-list-with-menu")]
		public async Task<IActionResult> GetListWithMenu([FromQuery] GetAllQueryRequest request)
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

                var userId = GetCurrentUser() ?? Guid.Empty;
                var data = await Mediator.Send(new FeaturePermission_GetAllWithMenuQuery(userId, request));
				return Ok(data);
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpGet("get-user-permission")]
		public async Task<IActionResult> GetUserPermission()
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var userId = GetCurrentUser() ?? Guid.Empty;
				var data = await Mediator.Send(new FeaturePermission_GetByUserQuery(userId));
				return Ok(data);
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpGet("get-policy")]
		public async Task<IActionResult> GetPolicy(Guid Id)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var user = GetCurrentUser() ?? Guid.Empty;
				var tenant = GetCurrentTenant();
				var data = await _permissionService.GetPolicy(Id, tenant);
				return Ok(Result<List<string>>.Success(data));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}
	}
}
