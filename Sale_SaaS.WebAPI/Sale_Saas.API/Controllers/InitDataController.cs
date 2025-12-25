using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Services.Identity;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class InitDataController : BaseController
	{
		private readonly ILoggerService	_loggerService;
		private readonly IApplicationDbContextInitialiser _initDataService;

		public InitDataController(ILoggerService loggerService, IApplicationDbContextInitialiser initDataService)
		{
			_loggerService = loggerService;
			_initDataService = initDataService;
		}

		[HttpPost("test")]
		[AllowAnonymous]
		public async Task<IActionResult> InitRoleaDetail(RolePositionEnum? role = null)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var tmp = FeatureDataConstant.sale_cohoi_feature(role);


                return Ok(tmp);
            }
            catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpPost("init-role-detail")]
		public async Task<IActionResult> InitRoleDetail()
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				
				return Ok(await _initDataService.InitApplicationRoleDetail());
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpPost("role-position-feature-menu")]
		public async Task<IActionResult> InitRolePositionFeatureMenu()
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var user = GetCurrentUser() ?? Guid.Empty;
				return Ok(await _initDataService.InitRolePositionFeatureMenu());
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpPost("init-menu")]
		public async Task<IActionResult> InitMenu()
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var user = GetCurrentUser() ?? Guid.Empty;
				return Ok(await _initDataService.InitMenu());
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpPost("init-feature-menu")]
		public async Task<IActionResult> InitFeatureMenu()
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var user = GetCurrentUser() ?? Guid.Empty;
				return Ok(await _initDataService.InitFeatureMenu());
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpPost("init-feature")]
		public async Task<IActionResult> InitFeature()
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var user = GetCurrentUser() ?? Guid.Empty;
				return Ok(await _initDataService.InitFeature());
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpPost("init-full-permission")]
		public async Task<IActionResult> InitFullPermission()
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				int rows = 0;
				rows += await _initDataService.InitMenu();
				rows += await _initDataService.InitFeature();
				rows += await _initDataService.InitFeatureMenu();
				rows += await _initDataService.InitRolePositionFeatureMenu();
				rows += await _initDataService.InitApplicationRoleDetail();

				return Ok(rows);
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}
	}
}
