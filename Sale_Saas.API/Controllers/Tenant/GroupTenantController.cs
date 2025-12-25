using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.BenefitFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using System.Reflection;
using Sale_Saas.Application.Features.BenefitFeature.Commands;
using Sale_Saas.Application.Models.GroupTenant;
using Sale_Saas.Application.Features.BenefitStatusFeature.Queries;
using Sale_Saas.Infrastructure.Services.TenantService;
using Microsoft.AspNetCore.Authorization;

namespace Sale_Saas.API.Controllers.Tenant
{
	[Route("api/[controller]")]
	[ApiController]
	public class GroupTenantController : BaseController
	{
		private readonly ILoggerService _loggerService;
		private readonly IGroupTenantService _groupTenantService;

		public GroupTenantController(ILoggerService loggerService, IGroupTenantService groupTenantService)
		{
			_loggerService = loggerService;
			_groupTenantService = groupTenantService;
		}

        [HttpGet("get-by-domain")]
		[AllowAnonymous]
        public async Task<IActionResult> GetById(string domain)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return Ok(await _groupTenantService.FindByDomainAsync(domain));
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
				return Ok(await _groupTenantService.GetById(id));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpGet("get-list-with-pagination")]
		public async Task<IActionResult> GetListWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				return Ok(await _groupTenantService.GetListWithPaginationQuery(request));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpDelete("delete-by-ids/{ids}")]
		public async Task<IActionResult> DeleteByIds(string ids)
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
					ApplicationUserId = Guid.Empty
				};

				var userId = GetCurrentUser() ?? Guid.Empty;

				return Ok(await _groupTenantService.DeleteByIds(request));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpPost("add-group-tenant")]
		public async Task<IActionResult> Add([FromForm] GroupTenantAddOrUpdateRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var userId = GetCurrentUser() ?? Guid.Empty;

				return Ok(await _groupTenantService.AddAsync(request, userId));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		//[HttpPut("update-group-tenant")]
		//public async Task<IActionResult> Update([FromForm] GroupTenantAddOrUpdateRequest request)
		//{
		//	try
		//	{
		//		if (!ModelState.IsValid)
		//		{
		//			return BadRequest(ModelState);
		//		}

  //              var userId = GetCurrentUser() ?? Guid.Empty;

  //              return Ok(await _groupTenantService.UpdateAsync(request, userId));
		//	}
		//	catch (Exception ex)
		//	{
		//		_loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod().Name);
		//		return Ok(Result<string>.Failure(ex.Message));
		//	}
		//}

		[HttpGet("extend-plan-service")]
		public async Task<IActionResult> ExtendPlanService(Guid id)
		{
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return Ok(await _groupTenantService.ExtendPlanService(id));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

		[HttpPost("add-extend-plan-service")]
        public async Task<IActionResult> AddExtendPlanService(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

				var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await _groupTenantService.AddExtendPlanService(userId, id));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

		[HttpPut("update-plan-service")]
        public async Task<IActionResult> UpdatePlanServiceOfGroupTenant(GroupTenantUpdatePlanServiceRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await _groupTenantService.UpdatePlanServiceAsync(userId, request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("update-group-tenant")]
        public async Task<IActionResult> UpdateGroupTenant([FromForm] GroupTenantAddOrUpdateBaseRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await _groupTenantService.UpdateGroupTenantAsync(userId, request));
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
                return Ok(await _groupTenantService.UpdatePassword(request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
