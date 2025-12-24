using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.SyllabusFeature.Requests;
using Sale_Saas.Application.Features.UnitFeature.Commands;
using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Features.UnitFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitController : BaseController
    {
        private readonly ILoggerService _loggerService;

        public UnitController(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpPost("add-unit")]
        public async Task<IActionResult> AddUnit([FromForm] AddUnit request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Unit_AddCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("{syllabusId}/get-list-unit-by-admin")]
        public async Task<IActionResult> GetListUnitWithAdminBySyllabus(Guid syllabusId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Unit_GetListBySyllabusIdQuery(userId, syllabusId)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-unit-by-id")]
        public async Task<IActionResult> GetUnitById(Guid unitId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Unit_GetUnitById(userId, unitId)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("update-unit")]
        public async Task<IActionResult> UpdateUnit([FromForm] UpdateUnit request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Unit_UpdateCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-unit")]
        public async Task<IActionResult> DeleteUnit(Guid unitId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Unit_DeleteCommand(userId, unitId)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-list-other-unit")]
        public async Task<IActionResult> GetListOtherUnit()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Unit_GetListOtherUnitQuery(userId)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("{syllabusId}/get-list-unit-pagination-by-user")]
        public async Task<IActionResult> GetListUnitPaginationBySyllabusId(Guid syllabusId, GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Unit_GetListWithPaginationByUserQuery(userId, syllabusId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("get-list-by-admin")]
        public async Task<IActionResult> GetListUnitWithAdmin([FromBody] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Unit_GetListUnitWithAdminQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("check-prerequisite-unit")]
        public async Task<IActionResult> CheckPrerequisiteUnitOfUser([FromBody] CheckUnitPrerequisiteRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Unit_CheckPrerequisiteUnitOfUserQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("create-or-update-prerequisite-unit")]
        public async Task<IActionResult> CreateOrUpdatePrerequisiteUnit([FromBody] CreateOrUpdateUnitPrerequisiteRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Unit_CreateOrUpdatePrerequisiteUnitCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("is-allowed-learn")]
        public async Task<IActionResult> CheckUnitAllowedLearn(SyllabusUnitRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Unit_CheckUnitAllowedLearnQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("{unitId}/get-list-result")]
        public async Task<IActionResult> MobileGetListResult(Guid unitId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new UnitMobile_GetDetailBySaleDirectorQuery(userId, unitId)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
