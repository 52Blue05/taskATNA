using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.CriteriaFeature.Commands;
using Sale_Saas.Application.Features.CriteriaFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using System.Reflection;

namespace Sale_Saas.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CriteriaController : BaseController
{
    private readonly ILoggerService _loggerService;
    public CriteriaController(ILoggerService loggerService)
    {
        _loggerService = loggerService;
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

            var userId = GetCurrentUser() ?? Guid.Empty;

            return Ok(await Mediator.Send(new Criteria_GetByIdQuery(userId, id)));
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

            var userId = GetCurrentUser() ?? Guid.Empty;

            return Ok(await Mediator.Send(new Criteria_GetListWithPaginationQuery(userId, request)));
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

            var userId = GetCurrentUser() ?? Guid.Empty;

            return Ok(await Mediator.Send(new Criteria_AddOrUpdateCommand(userId, request)));
        }
        catch (Exception ex)
        {
            _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

            return Ok(Result<string>.Failure(ex.Message));
        }
    }

    [HttpDelete("delete-by-ids")]
    public async Task<IActionResult> DeleteByIds([FromBody] DeleteRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUser() ?? Guid.Empty;

            return Ok(await Mediator.Send(new Criteria_DeleteCommand(userId, request)));
        }
        catch (Exception ex)
        {
            _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

            return Ok(Result<string>.Failure(ex.Message));
        }
    }
}
