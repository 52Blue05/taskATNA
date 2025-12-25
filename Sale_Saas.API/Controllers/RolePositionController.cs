using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.RolePositionFeature.Commands;
using Sale_Saas.Application.Features.RolePositionFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RolePositionController : BaseController
	{
		private readonly ILoggerService _loggerService;

		public RolePositionController(ILoggerService loggerService)
		{
			_loggerService = loggerService;
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
				var user = GetCurrentUser() ?? Guid.Empty;
				/*var access = await _permissionService.HasPermission(user, MenuEnum.DM_KH.ToString(), PermissionType.permission.ViewAll);
				if (access == false) throw new ApplicationException("Tài khoản không đủ quyền truy cập");*/

				return Ok(await Mediator.Send(new RolePosition_GetAllQuery(request)));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpGet("get-by-id/{id}")]
		public async Task<IActionResult> GetById(string id)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				/*var user = GetCurrentUser() ?? Guid.Empty;
				var access = await _permissionService.HasPermission(user, MenuEnum.DM_KH.ToString(), PermissionType.permission.ViewProfile);
				if (access == false) throw new ApplicationException("Tài khoản không đủ quyền truy cập");*/

				return Ok(await Mediator.Send(new RolePosition_GetByIdQuery(id)));
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

				return Ok(await Mediator.Send(new RolePosition_AddOrUpdateCommand(user, request)));
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

				/*var user = GetCurrentUser() ?? Guid.Empty;
				var access = await _permissionService.HasPermission(user, MenuEnum.DM_KH.ToString(), PermissionType.permission.Delete);
				if (access == false) throw new ApplicationException("Tài khoản không đủ quyền truy cập");*/

				return Ok(await Mediator.Send(new RolePosition_DeleteByIdCommand(request)));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}
	}
}
