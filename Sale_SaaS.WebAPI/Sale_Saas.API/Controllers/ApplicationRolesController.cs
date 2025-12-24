
using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.BenefitFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Infrastructure.Authentication;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    public class ApplicationRolesController : BaseController
    {
        private readonly IApplicationRoleService _applicationRoleService;
        private readonly ILoggerService _loggerService;

        public ApplicationRolesController(IApplicationRoleService applicationRoleService, ILoggerService loggerService)
        {
            _applicationRoleService = applicationRoleService;
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

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await _applicationRoleService.GetAllQuery(request, userId));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
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

                return Ok(await _applicationRoleService.GetListWithPaginationQuery(request));
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

                return Ok(await _applicationRoleService.GetById(id));
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

                return Ok(await _applicationRoleService.AddOrUpdateAsync(request));
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

                return Ok(await _applicationRoleService.DeleteByIds(request));
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

                return Ok(await _applicationRoleService.FilterQuery(request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-role-by-role-position")]
        public async Task<IActionResult> GetRoleByRolePosition([FromQuery] string RolePositionId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await _applicationRoleService.GetApplicationRoleByRolePositionId(RolePositionId));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-list-role-sale-kit-by-role-position")]
        public async Task<IActionResult> GetListRoleSaleKitByRolePosition([FromQuery] string RolePositionId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await _applicationRoleService.GetListRoleSaleKitByRolePositionId(RolePositionId));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-role-by-user")]
        public async Task<IActionResult> GetRoleByUserHasNotBenefit([FromQuery] Guid UserId, [FromQuery] string RolePositionId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await _applicationRoleService.GetListRoleByUser(UserId, RolePositionId));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
