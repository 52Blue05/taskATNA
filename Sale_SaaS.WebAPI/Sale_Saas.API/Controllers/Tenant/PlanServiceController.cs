using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Infrastructure.Services.TenantService;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;
using System.Reflection;

namespace Sale_Saas.API.Controllers.Tenant
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanServiceController : BaseController
    {
        private readonly  ILoggerService _loggerService;
        private  IPlanService _planService;

        public PlanServiceController(ILoggerService loggerService, IPlanService planService)
        {
            _loggerService = loggerService;
            _planService = planService;
        }

        [HttpPost("add-plan-service")]
        public async Task<IActionResult> AddPlanService(CreatePlanServiceDto request)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return Ok(Result<PlanServiceDto>.Failure(ModelState.ToString() ?? ""));
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await _planService.CreatePlanService(userId, request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<PlanServiceDto>.Failure(ex.Message));
            }
        }

        [HttpGet("get-list-all-with-pagination")]
        public async Task<IActionResult> GetListAllWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(Result<PaginatedList<PlanServiceDto>>.Failure(ModelState.ToString() ?? ""));
                }

                //var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await _planService.GetListAllWithPagination(request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<PaginatedList<PlanServiceDto>>.Failure(ex.Message));
            }
        }

        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(Result<PlanServiceDto>.Failure(ModelState.ToString() ?? ""));
                }

                return Ok(await _planService.GetPlanServiceById(id));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<PlanServiceDto>.Failure(ex.Message));
            }
        }

        [HttpPost("update-plan-service")]
        public async Task<IActionResult> UpdatePlanService(UpdatePlanServiceDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(Result<PlanServiceDto>.Failure(ModelState.ToString() ?? ""));
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await _planService.UpdatePlanService(userId, request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<PlanServiceDto>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}")]
        public async Task<IActionResult> DeleteById(string ids)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(Result<PlanServiceDto>.Failure(ModelState.ToString() ?? ""));
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                List<string> tempList = ids.Split(',').ToList();  
                List<Guid> listIds = tempList.Select(s => Guid.Parse(s)).ToList();

                return Ok(await _planService.DeletePlanServiceById(userId, listIds));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<PlanServiceDto>.Failure(ex.Message));
            }
        }

        [HttpGet("get-list-plan-service-for-add-group-tenant")]
        public async Task<IActionResult> GetListPlanServiceForAddGroupTenant()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(Result<List<PlanServiceBasic>>.Failure(ModelState.ToString() ?? ""));
                }

                return Ok(await _planService.GetPlanServicForAddGroupTenant());
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<List<PlanServiceBasic>>.Failure(ex.Message));
            }
        }

        [HttpGet("get-list-plan-service-for-change-plan-service")]
        public async Task<IActionResult> GetListPlanServiceForChangePlanService(Guid groupTenantId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(Result<ChangePlanServiceDto>.Failure(ModelState.ToString() ?? ""));
                }

                return Ok(await _planService.GetPlanServicForChangePlanService(groupTenantId));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<ChangePlanServiceDto>.Failure(ex.Message));
            }
        }
    }
}
