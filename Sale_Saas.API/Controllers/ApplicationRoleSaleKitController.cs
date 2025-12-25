using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.ApplicationRoleSaleKitFeature.Commands;
using Sale_Saas.Application.Features.ApplicationRoleSaleKitFeature.Queries;
using Sale_Saas.Application.Features.SaleKitFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Models.Role_SaleKit;
using Sale_Saas.Application.Models.SaleKit;
using Sale_Saas.Infrastructure.Authentication;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationRoleSaleKitController : BaseController
    {
        private readonly ILoggerService _loggerService;

        public ApplicationRoleSaleKitController(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpGet("get-all")]
		//[HasPermission(PolicyTypes.Sale_SK_PQ.SALEKIT_XEMPHANQUYEN)]
		public async Task<IActionResult> GetAll([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new ApplicationRoleSaleKit_GetAllQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-list-with-pagination")]
		//[HasPermission(PolicyTypes.Sale_SK_PQ.SALEKIT_XEMPHANQUYEN)]
		public async Task<IActionResult> GetListWithPagination([FromQuery] RoleSaleKitGetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new ApplicationRoleSaleKit_GetListWithPaginationQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("update")]
		//[HasPermission(PolicyTypes.Sale_SK_PQ.SALEKIT_CAPNHATPHANQUYEN)]
		public async Task<IActionResult> Update([FromBody] SaleKitUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new ApplicationRoleSaleKit_AddOrUpdateCommand( userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-list-with-pagination-by-role")]
        //[HasPermission(PolicyTypes.Sale_SK_PQ.SALEKIT_XEMPHANQUYEN)]
        public async Task<IActionResult> GetListWithPaginationByRole([FromQuery] RoleSaleKitGetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new SaleKit_GetListWithPaginationByRoleQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
