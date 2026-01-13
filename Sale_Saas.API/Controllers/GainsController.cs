using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.GainsFamilyFeature.Commands;
using Sale_Saas.Application.Features.GainsFeature.Commands;
using Sale_Saas.Application.Features.GainsFeature.Queries;
using Sale_Saas.Application.Features.GainsFeature.RequestModels;
using Sale_Saas.Application.Features.GainsSchoolFeature.Commads;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Infrastructure.Authentication;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GainsController : BaseController
    {
        private readonly ILoggerService _loggerService;

        public GainsController(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpGet("get-by-relationship-id/{id}")]
        [HasPermission(PolicyTypes.Sale_MQH.MQH_CAPNHATBANGGAINS)]
        public async Task<IActionResult> GetByRelationshipId(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Gains_GetByRelationshipIdQuery(userId, id)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("add-or-update")]
        [HasPermission(PolicyTypes.Sale_MQH.MQH_CAPNHATBANGGAINS)]
        public async Task<IActionResult> AddOrUpdate([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Gains_AddOrUpdateCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/add-or-update")]
        [HasPermission(PolicyTypes.Sale_MQH.MQH_CAPNHATBANGGAINS)]
        public async Task<IActionResult> AddOrUpdateV2([FromBody] GainsMobile_AddOrUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Gains_AddOrUpdate_V2Command(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-by-id/{id}")]
        [HasPermission(PolicyTypes.Sale_MQH.MQH_CAPNHATBANGGAINS)]
        public async Task<IActionResult> MobileGetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new GainsMobile_GetByIdQuery(userId, id)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
        [HasPermission(PolicyTypes.Sale_MQH.MQH_CAPNHATBANGGAINS)]
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

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Gains_DeleteByIdCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("gains-schools/delete-by-ids")]
        public async Task<IActionResult> GainsSchoolDeleteByIds([FromBody] DeleteRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new GainsSchool_DeleteByIdsCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("gains-families/delete-by-ids")]
        public async Task<IActionResult> GainsFamiliesDeleteByIds([FromBody] DeleteRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new GainsFamily_DeleteByIdsCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
